using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.SubscriptionPlans
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public GymProgram GymProgram { get; set; } = default!;
        public List<SubscriptionPlan> Plans { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int gymProgramId)
        {
            var program = await _context.GymPrograms
                .Include(g => g.Trainer)
                .FirstOrDefaultAsync(g => g.Id == gymProgramId);

            if (program == null)
                return NotFound();

            GymProgram = program;

            Plans = await _context.SubscriptionPlans
                .Where(p => p.GymProgramId == gymProgramId)
                .OrderBy(p => p.SessionsPerMonth)
                .ToListAsync();

            return Page();
        }
    }
}
