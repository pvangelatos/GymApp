using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.TimeSlots
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public GymProgram GymProgram { get; set; } = default!;
        public List<TimeSlot> MorningSlots { get; set; } = new();
        public List<TimeSlot> AfternoonSlots { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int gymProgramId)
        {
            var program = await _context.GymPrograms
                .Include(g => g.Trainer)
                .FirstOrDefaultAsync(g => g.Id == gymProgramId);

            if (program == null)
                return NotFound();

            GymProgram = program;

            var allSlots = await _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            MorningSlots = allSlots.Where(t => t.SessionType == SessionType.Morning).ToList();
            AfternoonSlots = allSlots.Where(t => t.SessionType == SessionType.Afternoon).ToList();

            return Page();
        }
    }
}
