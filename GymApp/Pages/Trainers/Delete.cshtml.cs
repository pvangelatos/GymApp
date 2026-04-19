using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.Trainers
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Trainer Trainer { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer == null)
                return NotFound();

            Trainer = trainer;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);

            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}
