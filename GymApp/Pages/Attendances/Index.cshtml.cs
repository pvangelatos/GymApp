using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Attendances
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public Subscription Subscription { get; set; } = default!;
        public List<Attendance> Attendances { get; set; } = new();
        public int RemainingSessions { get; set; }

        public async Task<IActionResult> OnGetAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null)
                return NotFound();

            Subscription = subscription;

            Attendances = await _context.Attendances
                .Where(a => a.SubscriptionId == subscriptionId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            RemainingSessions = subscription.SubscriptionPlan.SessionsPerMonth - Attendances.Count;

            return Page();
        }
    }
}
