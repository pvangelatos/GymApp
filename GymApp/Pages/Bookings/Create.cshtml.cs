using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Bookings
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Booking Booking { get; set; } = new();

        [BindProperty]
        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public Subscription Subscription { get; set; } = default!;
        public SelectList? TimeSlotList { get; set; }
        public bool DateSelected { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(int subscriptionId)
        {
            var subscription = await LoadSubscriptionAsync(subscriptionId);
            if (subscription == null) return NotFound();

            Subscription = subscription;
            Booking.SubscriptionId = subscriptionId;
            return Page();
        }

        // Βήμα 1: Επιλογή ημερομηνίας
        public async Task<IActionResult> OnPostSelectDateAsync()
        {
            ModelState.Clear();

            var subscription = await LoadSubscriptionAsync(Booking.SubscriptionId);
            Subscription = subscription!;

            await LoadTimeSlotsAsync(subscription!, SelectedDate);
            DateSelected = true;
            return Page();
        }

        // Βήμα 2: Αποθήκευση κράτησης
        public async Task<IActionResult> OnPostSaveAsync()
        {
            ModelState.Remove("Booking.Subscription");
            ModelState.Remove("Booking.TimeSlot");

            var subscription = await LoadSubscriptionAsync(Booking.SubscriptionId);

            if (!ModelState.IsValid)
            {
                Subscription = subscription!;
                await LoadTimeSlotsAsync(subscription!, SelectedDate);
                DateSelected = true;
                return Page();
            }

            // Έλεγχος αν η ημερομηνία ταιριάζει με την ημέρα του slot
            var slot = await _context.TimeSlots.FindAsync(Booking.TimeSlotId);
            if ((int)SelectedDate.DayOfWeek != (int)slot!.DayOfWeek)
            {
                ModelState.AddModelError("", $"Η ημερομηνία δεν ταιριάζει με την ημέρα του slot.");
                Subscription = subscription!;
                await LoadTimeSlotsAsync(subscription!, SelectedDate);
                DateSelected = true;
                return Page();
            }

            // Έλεγχος χωρητικότητας
            var bookingsForSlot = await _context.Bookings
                .CountAsync(b => b.TimeSlotId == Booking.TimeSlotId
                    && b.BookingDate.Date == SelectedDate.Date
                    && (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Attended));

            if (bookingsForSlot >= slot.Capacity)
            {
                ModelState.AddModelError("", "Το slot είναι πλήρες για αυτή την ημερομηνία.");
                Subscription = subscription!;
                await LoadTimeSlotsAsync(subscription!, SelectedDate);
                DateSelected = true;
                return Page();
            }

            Booking.BookingDate = SelectedDate;
            Booking.Status = BookingStatus.Booked;
            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { subscriptionId = Booking.SubscriptionId });
        }

        private async Task<Subscription?> LoadSubscriptionAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
        }

        private async Task LoadTimeSlotsAsync(Subscription subscription, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var slots = await _context.TimeSlots
                .Where(t => t.GymProgramId == subscription.SubscriptionPlan.GymProgramId
                    && t.SessionType == subscription.SessionType
                    && t.DayOfWeek == dayOfWeek)
                .OrderBy(t => t.StartTime)
                .ToListAsync();

            // Φιλτράρισμα πλήρων slots
            var availableSlots = new List<object>();
            foreach (var s in slots)
            {
                var bookingCount = await _context.Bookings
                    .CountAsync(b => b.TimeSlotId == s.Id
                        && b.BookingDate.Date == date.Date
                        && (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Attended));

                var label = s.StartTime.ToString("HH:mm");
                if (bookingCount >= s.Capacity)
                    label += " (Πλήρες)";
                else
                    label += $" ({s.Capacity - bookingCount} θέσεις)";

                availableSlots.Add(new { s.Id, Label = label, IsFull = bookingCount >= s.Capacity });
            }

            TimeSlotList = new SelectList(
                availableSlots.Where(s => !(bool)s.GetType().GetProperty("IsFull")!.GetValue(s)!),
                "Id", "Label");
        }

        private string DayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Δευτέρα",
            DayOfWeek.Tuesday => "Τρίτη",
            DayOfWeek.Wednesday => "Τετάρτη",
            DayOfWeek.Thursday => "Πέμπτη",
            DayOfWeek.Friday => "Παρασκευή",
            DayOfWeek.Saturday => "Σάββατο",
            DayOfWeek.Sunday => "Κυριακή",
            _ => day.ToString()
        };
    }
}