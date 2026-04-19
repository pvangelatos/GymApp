using GymApp.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Reports
{
    public class RevenueModel : PageModel
    {
        private readonly AppDbContext _context;

        public RevenueModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MonthlyRevenue> MonthlyData { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public decimal AverageMonthlyRevenue { get; set; }

        public async Task OnGetAsync()
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .ToListAsync();

            MonthlyData = subscriptions
                .GroupBy(s => new { s.StartDate.Year, s.StartDate.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(s => s.AmountPaid),
                    SubscriptionCount = g.Count()
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            TotalRevenue = MonthlyData.Sum(m => m.Revenue);
            AverageMonthlyRevenue = MonthlyData.Any()
                ? TotalRevenue / MonthlyData.Count
                : 0;
        }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public int SubscriptionCount { get; set; }
        public string MonthName => new DateTime(Year, Month, 1)
            .ToString("MMMM yyyy", new System.Globalization.CultureInfo("el-GR"));
    }
}
