using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.GymPrograms
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GymProgram GymProgram { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var program = await _context.GymPrograms
                .Include(g => g.Trainer)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (program == null)
                return NotFound();

            GymProgram = program;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var program = await _context.GymPrograms.FindAsync(id);

            if (program != null)
            {
                _context.GymPrograms.Remove(program);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}