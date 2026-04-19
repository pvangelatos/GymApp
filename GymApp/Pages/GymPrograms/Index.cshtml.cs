using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.GymPrograms
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<GymProgram> GymPrograms { get; set; } = new();

        public async Task OnGetAsync()
        {
            GymPrograms = await _context.GymPrograms
                .Include(g => g.Trainer)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
}
