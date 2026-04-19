using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.GymPrograms
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GymProgram GymProgram { get; set; } = new();

        public SelectList TrainerList { get; set; } = default!;

        public async Task OnGetAsync()
        {
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("GymProgram.Trainer");

            if (!ModelState.IsValid)
            {
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

            _context.GymPrograms.Add(GymProgram);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
