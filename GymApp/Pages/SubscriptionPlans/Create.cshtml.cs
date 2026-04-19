using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.SubscriptionPlans
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SubscriptionPlan Plan { get; set; } = new();

        public GymProgram GymProgram { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int gymProgramId)
        {
            var program = await _context.GymPrograms.FindAsync(gymProgramId);

            if (program == null)
                return NotFound();

            GymProgram = program;
            Plan.GymProgramId = gymProgramId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Plan.GymProgram");

            if (!ModelState.IsValid)
            {
                var program = await _context.GymPrograms.FindAsync(Plan.GymProgramId);
                GymProgram = program!;
                return Page();
            }

            _context.SubscriptionPlans.Add(Plan);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { gymProgramId = Plan.GymProgramId });
        }
    }
}
