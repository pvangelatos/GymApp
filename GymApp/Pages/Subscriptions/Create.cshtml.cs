using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subscription Subscription { get; set; } = new();

        [BindProperty]
        public int SelectedProgramId { get; set; }

        public Member Member { get; set; } = default!;
        public SelectList ProgramList { get; set; } = default!;
        public SelectList? PlanList { get; set; }
        public SelectList? SessionTypeList { get; set; }
        public bool ProgramSelected { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return NotFound();

            Member = member;
            Subscription.MemberId = memberId;
            Subscription.StartDate = DateTime.Today;

            await LoadProgramListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostSelectProgramAsync()
        {
            ModelState.Clear();

            var member = await _context.Members.FindAsync(Subscription.MemberId);
            Member = member!;

            await LoadProgramListAsync();
            await LoadPlanListAsync(SelectedProgramId);
            LoadSessionTypeList(SelectedProgramId);

            ProgramSelected = true;
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            ModelState.Remove("Subscription.Member");
            ModelState.Remove("Subscription.SubscriptionPlan");

            if (!ModelState.IsValid)
            {
                var member = await _context.Members.FindAsync(Subscription.MemberId);
                Member = member!;
                await LoadProgramListAsync();
                await LoadPlanListAsync(SelectedProgramId);
                LoadSessionTypeList(SelectedProgramId);
                ProgramSelected = true;
                return Page();
            }

            // Έλεγχος ενεργής συνδρομής στο ίδιο πρόγραμμα
            var existingActive = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.MemberId == Subscription.MemberId
                    && s.IsActive
                    && s.SubscriptionPlan.GymProgramId == SelectedProgramId)
                .FirstOrDefaultAsync();

            if (existingActive != null)
            {
                ModelState.AddModelError("", "Το μέλος έχει ήδη ενεργή συνδρομή σε αυτό το πρόγραμμα.");
                var member = await _context.Members.FindAsync(Subscription.MemberId);
                Member = member!;
                await LoadProgramListAsync();
                await LoadPlanListAsync(SelectedProgramId);
                LoadSessionTypeList(SelectedProgramId);
                ProgramSelected = true;
                return Page();
            }

            // Έλεγχος διαθέσιμων θέσεων
            var availableSlots = await GetAvailableSlotsAsync(SelectedProgramId, Subscription.SessionType);
            var plan = await _context.SubscriptionPlans.FindAsync(Subscription.SubscriptionPlanId);
            var requiredSlots = plan!.SessionsPerMonth / 4;

            if (availableSlots < requiredSlots)
            {
                ModelState.AddModelError("", $"Δεν υπάρχουν αρκετές διαθέσιμες θέσεις. Διαθέσιμες: {availableSlots}, Απαιτούμενες: {requiredSlots}.");
                var member = await _context.Members.FindAsync(Subscription.MemberId);
                Member = member!;
                await LoadProgramListAsync();
                await LoadPlanListAsync(SelectedProgramId);
                LoadSessionTypeList(SelectedProgramId);
                ProgramSelected = true;
                return Page();
            }

            Subscription.EndDate = Subscription.StartDate.AddMonths(1);
            Subscription.AmountPaid = plan!.Price;
            Subscription.IsActive = true;

            _context.Subscriptions.Add(Subscription);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { memberId = Subscription.MemberId });
        }

        private async Task<int> GetAvailableSlotsAsync(int gymProgramId, SessionType sessionType)
        {
            // Συνολικές θέσεις/εβδομάδα για το τμήμα
            var totalSlots = await _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId && t.SessionType == sessionType)
                .SumAsync(t => t.Capacity);

            // Δεσμευμένες θέσεις από ενεργές συνδρομές
            var occupiedSlots = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.IsActive
                    && s.SessionType == sessionType
                    && s.SubscriptionPlan.GymProgramId == gymProgramId)
                .SumAsync(s => s.SubscriptionPlan.SessionsPerMonth / 4);

            return totalSlots - occupiedSlots;
        }

        private async Task LoadProgramListAsync()
        {
            var programs = await _context.GymPrograms
                .OrderBy(g => g.Name)
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();

            ProgramList = new SelectList(programs, "Id", "Name", SelectedProgramId);
        }

        private async Task LoadPlanListAsync(int programId)
        {
            var plans = await _context.SubscriptionPlans
                .Where(p => p.GymProgramId == programId)
                .OrderBy(p => p.SessionsPerMonth)
                .Select(p => new
                {
                    p.Id,
                    Description = p.SessionsPerMonth + " συνεδρίες — " + p.Price + "€"
                })
                .ToListAsync();

            PlanList = new SelectList(plans, "Id", "Description");
        }

        private void LoadSessionTypeList(int gymProgramId)
        {
            var availableSessionTypes = _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId)
                .Select(t => t.SessionType)
                .Distinct()
                .ToList();

            var sessionTypeItems = new List<object>();
            if (availableSessionTypes.Contains(SessionType.Morning))
                sessionTypeItems.Add(new { Value = "Morning", Text = "Πρωί" });
            if (availableSessionTypes.Contains(SessionType.Afternoon))
                sessionTypeItems.Add(new { Value = "Afternoon", Text = "Απόγευμα" });

            // Αν δεν υπάρχουν TimeSlots εμφάνισε και τα δύο
            if (!sessionTypeItems.Any())
            {
                sessionTypeItems.Add(new { Value = "Morning", Text = "Πρωί" });
                sessionTypeItems.Add(new { Value = "Afternoon", Text = "Απόγευμα" });
            }

            SessionTypeList = new SelectList(sessionTypeItems, "Value", "Text");
        }
    }
}