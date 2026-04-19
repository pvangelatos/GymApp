using GymApp.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalPrograms { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ExpiredSubscriptions { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<(string ProgramName, int Count)> TopPrograms { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalMembers = await _context.Members.CountAsync();
            ActiveMembers = await _context.Members.CountAsync(m => m.IsActive);
            TotalPrograms = await _context.GymPrograms.CountAsync();

            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.IsActive);

            ExpiredSubscriptions = await _context.Subscriptions
                .CountAsync(s => !s.IsActive);

            var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            MonthlyRevenue = await _context.Subscriptions
                .Where(s => s.StartDate >= firstDayOfMonth)
                .SumAsync(s => s.AmountPaid);

            TopPrograms = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Where(s => s.IsActive)
                .GroupBy(s => s.SubscriptionPlan.GymProgram.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(g => (g.Name, g.Count)).ToList());
        }
    }
}
