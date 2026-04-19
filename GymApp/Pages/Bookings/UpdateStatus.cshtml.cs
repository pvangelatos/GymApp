using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Bookings
{
    public class UpdateStatusModel : PageModel
    {
        private readonly AppDbContext _context;

        public UpdateStatusModel(AppDbContext context)
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

        public async Task<IActionResult> OnPostAsync(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
            {
                booking.Status = Enum.Parse<BookingStatus>(status);
                await _context.SaveChangesAsync();
            }

            await CheckAndDeactivateSubscriptionAsync(booking.SubscriptionId);
            return RedirectToPage("Index", new { subscriptionId = booking?.SubscriptionId });
        }
    }
}
