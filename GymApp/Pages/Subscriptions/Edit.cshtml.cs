using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subscription Subscription { get; set; } = new();

        public Member Member { get; set; } = default!;
        public SelectList PlanList { get; set; } = default!;
        public SelectList SessionTypeList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) return NotFound();

            Subscription = subscription;
            Member = subscription.Member;

            await LoadListsAsync(subscription.SubscriptionPlan.GymProgramId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Subscription.Member");
            ModelState.Remove("Subscription.SubscriptionPlan");

            if (!ModelState.IsValid)
            {
                var sub = await _context.Subscriptions
                    .Include(s => s.Member)
                    .Include(s => s.SubscriptionPlan)
                    .FirstOrDefaultAsync(s => s.Id == Subscription.Id);
                Member = sub!.Member;
                await LoadListsAsync(sub.SubscriptionPlan.GymProgramId);
                return Page();
            }

            _context.Subscriptions.Update(Subscription);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { memberId = Subscription.MemberId });
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

            PlanList = new SelectList(plans, "Id", "Description", Subscription.SubscriptionPlanId);

            // Εμφάνισε μόνο SessionTypes που έχουν TimeSlots
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

            // Αν δεν υπάρχουν TimeSlots εμφάνισε και τα δύο
            if (!sessionTypeItems.Any())
            {
                sessionTypeItems.Add(new { Value = "Morning", Text = "Πρωί" });
                sessionTypeItems.Add(new { Value = "Afternoon", Text = "Απόγευμα" });
            }

            SessionTypeList = new SelectList(sessionTypeItems, "Value", "Text", Subscription.SessionType.ToString());
        }
    }
}
