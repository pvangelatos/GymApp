using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class RenewModel : PageModel
    {
        private readonly AppDbContext _context;

        public RenewModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subscription NewSubscription { get; set; } = new();

        public Subscription OldSubscription { get; set; } = default!;
        public SelectList PlanList { get; set; } = default!;
        public SelectList SessionTypeList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var oldSub = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (oldSub == null) return NotFound();

            OldSubscription = oldSub;

            // Προσυμπλήρωση νέας συνδρομής
            NewSubscription.MemberId = oldSub.MemberId;
            NewSubscription.SubscriptionPlanId = oldSub.SubscriptionPlanId;
            NewSubscription.SessionType = oldSub.SessionType;
            NewSubscription.StartDate = oldSub.EndDate;
            NewSubscription.EndDate = oldSub.EndDate.AddMonths(1);

            await LoadListsAsync(oldSub.SubscriptionPlan.GymProgramId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int oldSubscriptionId)
        {
            ModelState.Remove("NewSubscription.Member");
            ModelState.Remove("NewSubscription.SubscriptionPlan");

            var oldSub = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == oldSubscriptionId);

            if (!ModelState.IsValid)
            {
                OldSubscription = oldSub!;
                await LoadListsAsync(oldSub!.SubscriptionPlan.GymProgramId);
                return Page();
            }

            // Έλεγχος ενεργής συνδρομής στο ίδιο πρόγραμμα
            var gymProgramId = oldSub!.SubscriptionPlan.GymProgramId;
            var existingActive = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.MemberId == NewSubscription.MemberId
                    && s.IsActive
                    && s.SubscriptionPlan.GymProgramId == gymProgramId)
                .FirstOrDefaultAsync();

            if (existingActive != null)
            {
                ModelState.AddModelError("", "Υπάρχει ήδη ενεργή συνδρομή σε αυτό το πρόγραμμα.");
                OldSubscription = oldSub;
                await LoadListsAsync(gymProgramId);
                return Page();
            }

            // Έλεγχος διαθέσιμων θέσεων
            var plan = await _context.SubscriptionPlans.FindAsync(NewSubscription.SubscriptionPlanId);
            var requiredSlots = plan!.SessionsPerMonth / 4;
            var availableSlots = await GetAvailableSlotsAsync(gymProgramId, NewSubscription.SessionType);

            if (availableSlots < requiredSlots)
            {
                ModelState.AddModelError("", $"Δεν υπάρχουν αρκετές διαθέσιμες θέσεις. Διαθέσιμες: {availableSlots}, Απαιτούμενες: {requiredSlots}.");
                OldSubscription = oldSub;
                await LoadListsAsync(gymProgramId);
                return Page();
            }

            NewSubscription.AmountPaid = plan.Price;
            NewSubscription.IsActive = true;
            NewSubscription.EndDate = NewSubscription.StartDate.AddMonths(1);

            _context.Subscriptions.Add(NewSubscription);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { memberId = NewSubscription.MemberId });
        }

        private async Task<int> GetAvailableSlotsAsync(int gymProgramId, SessionType sessionType)
        {
            var totalSlots = await _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId && t.SessionType == sessionType)
                .SumAsync(t => t.Capacity);

            var occupiedSlots = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.IsActive
                    && s.SessionType == sessionType
                    && s.SubscriptionPlan.GymProgramId == gymProgramId)
                .SumAsync(s => s.SubscriptionPlan.SessionsPerMonth / 4);

            return totalSlots - occupiedSlots;
        }

        private async Task LoadListsAsync(int gymProgramId)
        {
            var plans = await _context.SubscriptionPlans
                .Where(p => p.GymProgramId == gymProgramId)
                .OrderBy(p => p.SessionsPerMonth)
                .Select(p => new
                {
                    p.Id,
                    Description = p.SessionsPerMonth + " συνεδρίες — " + p.Price + "€"
                })
                .ToListAsync();

            PlanList = new SelectList(plans, "Id", "Description", NewSubscription.SubscriptionPlanId);

            var availableSessionTypes = await _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId)
                .Select(t => t.SessionType)
                .Distinct()
                .ToListAsync();

            var sessionTypeItems = new List<object>();
            if (availableSessionTypes.Contains(SessionType.Morning))
                sessionTypeItems.Add(new { Value = "Morning", Text = "Πρωί" });
            if (availableSessionTypes.Contains(SessionType.Afternoon))
                sessionTypeItems.Add(new { Value = "Afternoon", Text = "Απόγευμα" });

            if (!sessionTypeItems.Any())
            {
                sessionTypeItems.Add(new { Value = "Morning", Text = "Πρωί" });
                sessionTypeItems.Add(new { Value = "Afternoon", Text = "Απόγευμα" });
            }

            SessionTypeList = new SelectList(sessionTypeItems, "Value", "Text", NewSubscription.SessionType.ToString());
        }
    }
}
