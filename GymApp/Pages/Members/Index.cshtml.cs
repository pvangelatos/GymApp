using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
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

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim().ToLower();
                query = query.Where(m =>
                    m.Firstname.ToLower().Contains(term) ||
                    m.Lastname.ToLower().Contains(term) ||
                    (m.Phone != null && m.Phone.Contains(term)));
            }

            if (StatusFilter == "active")
                query = query.Where(m => m.IsActive);
            else if (StatusFilter == "inactive")
                query = query.Where(m => !m.IsActive);

            Members = await query
                .OrderBy(m => m.Lastname)
                .ToListAsync();
        }
    }
}
