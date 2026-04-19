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

        // Όταν πατάς "Επιλογή Προγράμματος"
        public async Task<IActionResult> OnPostSelectProgramAsync()
        {
            ModelState.Clear();

            var member = await _context.Members.FindAsync(Subscription.MemberId);
            Member = member!;

            await LoadProgramListAsync();
            await LoadPlanListAsync(SelectedProgramId);

            ProgramSelected = true;
            return Page();
        }

        // Όταν πατάς "Αποθήκευση"
        public async Task<IActionResult> OnPostSaveAsync()
        {
            ModelState.Remove("Subscription.Member");
            ModelState.Remove("Subscription.SubscriptionPlan");
            ModelState.Remove("SelectedProgramId");

            if (!ModelState.IsValid)
            {
                
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        Console.WriteLine($"Field: {state.Key}, Error: {state.Value.Errors.First().ErrorMessage}");
                    }
                }

                var member = await _context.Members.FindAsync(Subscription.MemberId);
                Member = member!;
                await LoadProgramListAsync();
                await LoadPlanListAsync(SelectedProgramId);
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
                ProgramSelected = true;
                return Page();
            }

            var plan = await _context.SubscriptionPlans.FindAsync(Subscription.SubscriptionPlanId);
            Subscription.EndDate = Subscription.StartDate.AddMonths(1);
            Subscription.AmountPaid = plan!.Price;
            Subscription.IsActive = true;

            _context.Subscriptions.Add(Subscription);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { memberId = Subscription.MemberId });
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
    }
}