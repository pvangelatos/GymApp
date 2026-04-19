using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Reports
{
    public class ExpiringSubscriptionsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ExpiringSubscriptionsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Subscription> Subscriptions { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int DaysAhead { get; set; } = 7;

        public async Task OnGetAsync()
        {
            var targetDate = DateTime.Today.AddDays(DaysAhead);

            Subscriptions = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Where(s => s.IsActive && s.EndDate <= targetDate)
                .OrderBy(s => s.EndDate)
                .ToListAsync();
        }
    }
}
