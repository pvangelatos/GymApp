using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Trainers
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Trainer> Trainers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Trainers = await _context.Trainers
                .OrderBy(t => t.Lastname)
                .ToListAsync();
        }
    }
}
