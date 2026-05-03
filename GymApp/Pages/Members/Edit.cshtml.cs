using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Members
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;

        public EditModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Member Member { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound();

            Member = member;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Έλεγχος για duplicate email (εκτός του ίδιου μέλους)
            var exists = await _context.Members
                .AnyAsync(m => m.Email.ToLower() == Member.Email.ToLower()
                           && m.Id != Member.Id);

            if (exists)
            {
                ModelState.AddModelError("Member.Email", "Υπάρχει ήδη μέλος με αυτό το email.");
                return Page();
            }

            _context.Members.Update(Member);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}