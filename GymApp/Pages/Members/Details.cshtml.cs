using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.Members
{
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

        public Member Member { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound();

            Member = member;
            return Page();
        }
    }
}
