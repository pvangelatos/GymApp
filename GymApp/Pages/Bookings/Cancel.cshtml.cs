using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Bookings
{
    public class CancelModel : PageModel
    {
        private readonly AppDbContext _context;

        public CancelModel(AppDbContext context)
        {
            _context = context;
        }

        private async Task CheckAndDeactivateSubscriptionAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null) return;

            var usedSessions = subscription.Bookings.Count(b =>
                b.Status == BookingStatus.Attended ||
                b.Status == BookingStatus.NoShow ||
                b.Status == BookingStatus.Booked);

            if (usedSessions >= subscription.SubscriptionPlan.SessionsPerMonth)
            {
                subscription.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.TimeSlot)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking != null)
            {
                // Συνδυάζουμε ημερομηνία + ώρα μαθήματος
                var classDateTime = booking.BookingDate.Date
                    .AddHours(booking.TimeSlot.StartTime.Hour)
                    .AddMinutes(booking.TimeSlot.StartTime.Minute);

                var hoursUntilClass = (classDateTime - DateTime.Now).TotalHours;

                if (hoursUntilClass >= 24)
                {
                    // Ακύρωση εντός 24ωρών → δεν χάνει συνεδρία
                    booking.Status = BookingStatus.Cancelled;
                }
                else
                {
                    // Ακύρωση εκτός 24ωρών → NoShow, χάνει συνεδρία
                    booking.Status = BookingStatus.NoShow;
                }

                booking.CancelledAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            await CheckAndDeactivateSubscriptionAsync(booking!.SubscriptionId);

            return RedirectToPage("Index", new { subscriptionId = booking!.SubscriptionId });
        }
    }
}
