using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class AttendanceCardModel : PageModel
    {
        private readonly AppDbContext _context;

        public AttendanceCardModel(AppDbContext context)
        {
            _context = context;
        }

        public Subscription Subscription { get; set; } = default!;
        public List<SessionBox> SessionBoxes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Include(s => s.Bookings)
                    .ThenInclude(b => b.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) return NotFound();

            Subscription = subscription;

            // Δημιουργία κουτακιών βάσει SessionsPerMonth
            var bookings = subscription.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.BookingDate)
                .ToList();

            for (int i = 1; i <= subscription.SubscriptionPlan.SessionsPerMonth; i++)
            {
                var booking = i <= bookings.Count ? bookings[i - 1] : null;
                SessionBoxes.Add(new SessionBox
                {
                    Number = i,
                    Booking = booking
                });
            }

            return Page();
        }
    }

    public class SessionBox
    {
        public int Number { get; set; }
        public Booking? Booking { get; set; }
        public bool IsEmpty => Booking == null;
    }
}
