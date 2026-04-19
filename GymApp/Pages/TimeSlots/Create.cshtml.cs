using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymApp.Pages.TimeSlots
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TimeSlot TimeSlot { get; set; } = new();

        public GymProgram GymProgram { get; set; } = default!;

        public SelectList DayOfWeekList { get; set; } = default!;
        public SelectList SessionTypeList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int gymProgramId)
        {
            var program = await _context.GymPrograms.FindAsync(gymProgramId);
            if (program == null) return NotFound();

            GymProgram = program;
            TimeSlot.GymProgramId = gymProgramId;
            TimeSlot.Capacity = 5;

            LoadLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("TimeSlot.GymProgram");
            ModelState.Remove("TimeSlot.Bookings");

            if (!ModelState.IsValid)
            {
                var program = await _context.GymPrograms.FindAsync(TimeSlot.GymProgramId);
                GymProgram = program!;
                LoadLists();
                return Page();
            }

            _context.TimeSlots.Add(TimeSlot);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { gymProgramId = TimeSlot.GymProgramId });
        }

        private void LoadLists()
        {
            DayOfWeekList = new SelectList(new[]
            {
                new { Value = 1, Text = "Δευτέρα" },
                new { Value = 2, Text = "Τρίτη" },
                new { Value = 3, Text = "Τετάρτη" },
                new { Value = 4, Text = "Πέμπτη" },
                new { Value = 5, Text = "Παρασκευή" },
                new { Value = 6, Text = "Σάββατο" }
            }, "Value", "Text");

            SessionTypeList = new SelectList(new[]
            {
                new { Value = "Morning", Text = "Πρωί" },
                new { Value = "Afternoon", Text = "Απόγευμα" }
            }, "Value", "Text");
        }
    }
}
