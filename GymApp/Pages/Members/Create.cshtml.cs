using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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


            // Έλεγχος για duplicate email
            var exists = await _context.Members
                .AnyAsync(m => m.Email.ToLower() == Member.Email.ToLower());

            if (exists)
            {
                ModelState.AddModelError("Member.Email", "Υπάρχει ήδη μέλος με αυτό το email.");
                return Page();
            }

            Member.DateJoined = DateTime.Now;
            Member.IsActive = true;

            _context.Members.Add(Member);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
