using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.Members
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Member Member { get; set; } = new();
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Member.DateJoined = DateTime.Now;
            Member.IsActive = true;

            _context.Members.Add(Member);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
