using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.SubscriptionPlans
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SubscriptionPlan Plan { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var plan = await _context.SubscriptionPlans
                .Include(p => p.GymProgram)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return NotFound();

            Plan = plan;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);

            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { gymProgramId = Plan.GymProgramId });
        }
    }
}
