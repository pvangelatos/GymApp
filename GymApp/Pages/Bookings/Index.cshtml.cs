using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public Subscription Subscription { get; set; } = default!;
        public List<Booking> Bookings { get; set; } = new();
        public int RemainingSessions { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; } = new();

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

            Bookings = await _context.Bookings
                .Include(b => b.TimeSlot)
                .Where(b => b.SubscriptionId == subscriptionId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            var usedSessions = Bookings.Count(b =>
                b.Status == BookingStatus.Booked ||
                b.Status == BookingStatus.Attended ||
                b.Status == BookingStatus.NoShow);

            RemainingSessions = subscription.SubscriptionPlan.SessionsPerMonth - usedSessions;

            AvailableSlots = await _context.TimeSlots
                .Where(t => t.GymProgramId == subscription.SubscriptionPlan.GymProgramId
                    && t.SessionType == subscription.SessionType)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            return Page();
        }
    }
}
