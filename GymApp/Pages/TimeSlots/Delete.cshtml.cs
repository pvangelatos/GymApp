using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.TimeSlots
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TimeSlot TimeSlot { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var slot = await _context.TimeSlots
                .Include(t => t.GymProgram)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (slot == null) return NotFound();

            TimeSlot = slot;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var slot = await _context.TimeSlots.FindAsync(id);

            if (slot != null)
            {
                _context.TimeSlots.Remove(slot);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { gymProgramId = TimeSlot.GymProgramId });
        }
    }
}
