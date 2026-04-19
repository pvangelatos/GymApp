using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.SubscriptionPlans
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SubscriptionPlan Plan { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);

            if (plan == null)
                return NotFound();

            Plan = plan;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Plan.GymProgram");

            if (!ModelState.IsValid)
                return Page();

            _context.SubscriptionPlans.Update(Plan);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { gymProgramId = Plan.GymProgramId });
        }
    }
}
