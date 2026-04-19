using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.GymPrograms
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GymProgram GymProgram { get; set; } = new();

        public SelectList TrainerList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var program = await _context.GymPrograms.FindAsync(id);

            if (program == null)
                return NotFound();

            GymProgram = program;

            var trainers = await _context.Trainers
                .OrderBy(t => t.Lastname)
                .OrderBy(t => t.Lastname)
                    .Select(t => new
                    {
                        t.Id,
                        Fullname = t.Lastname + " " + t.Firstname
                    })
                    .ToListAsync();
            TrainerList = new SelectList(trainers, "Id", "Fullname");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("GymProgram.Trainer");

            if (!ModelState.IsValid)
            {
                var trainers = await _context.Trainers
                    .OrderBy(t => t.Lastname)
                    .Select(t => new
                    {
                        t.Id,
                        Fullname = t.Lastname + " " + t.Firstname
                    })
                    .ToListAsync();
                TrainerList = new SelectList(trainers, "Id", "Fullname");
                return Page();
            }

            _context.GymPrograms.Update(GymProgram);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}