using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Members
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Member> Members { get; set; } = new();

        public async Task OnGetAsync()
        {
            Members = await _context.Members
                .OrderBy(m => m.Lastname)
                .ToListAsync();
        }
    }
}
