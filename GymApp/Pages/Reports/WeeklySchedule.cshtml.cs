using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Reports
{
    public class WeeklyScheduleModel : PageModel
    {
        private readonly AppDbContext _context;

        public WeeklyScheduleModel(AppDbContext context)
        {
            _context = context;
        }

        public List<DayOfWeek> Days { get; set; } = new()
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

        public Dictionary<DayOfWeek, List<SlotInfo>> Schedule { get; set; } = new();

        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }

        [BindProperty]
        public int BookingSubscriptionId { get; set; }
        [BindProperty]
        public int BookingTimeSlotId { get; set; }
        [BindProperty]
        public DateTime BookingDate { get; set; }
        [BindProperty]
        public int WeekOffset { get; set; }

        public async Task<IActionResult> OnPostBookAsync()
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.Id == BookingSubscriptionId);

            if (subscription == null)
                return NotFound();

            // Έλεγχος αν υπάρχει ήδη κράτηση
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.SubscriptionId == BookingSubscriptionId
                    && b.BookingDate.Date == BookingDate.Date
                    && b.TimeSlotId == BookingTimeSlotId
                    && (b.Status == BookingStatus.Booked || b.Status == BookingStatus.Attended));

            if (!existingBooking)
            {
                // Έλεγχος εναπομεινασών συνεδριών
                var usedSessions = await _context.Bookings
                    .CountAsync(b => b.SubscriptionId == BookingSubscriptionId
                        && (b.Status == BookingStatus.Booked
                            || b.Status == BookingStatus.Attended
                            || b.Status == BookingStatus.NoShow));

                if (usedSessions < subscription.SubscriptionPlan.SessionsPerMonth)
                {
                    var booking = new Booking
                    {
                        SubscriptionId = BookingSubscriptionId,
                        TimeSlotId = BookingTimeSlotId,
                        BookingDate = BookingDate,
                        Status = BookingStatus.Booked
                    };
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage(new { weekOffset = WeekOffset });
        }

        public async Task OnGetAsync(int weekOffset = 0)
        {
            // Υπολογισμός τρέχουσας εβδομάδας
            var today = DateTime.Today;
            var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            WeekStart = today.AddDays(-daysUntilMonday + weekOffset * 7);
            WeekEnd = WeekStart.AddDays(5); // Σάββατο

            var timeSlots = await _context.TimeSlots
                .Include(t => t.GymProgram)
                    .ThenInclude(g => g.Trainer)
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            foreach (var day in Days)
            {
                var date = WeekStart.AddDays(((int)day - (int)DayOfWeek.Monday + 7) % 7);
                var daySlots = timeSlots.Where(t => t.DayOfWeek == day).ToList();
                var slotInfos = new List<SlotInfo>();

                foreach (var slot in daySlots)
                {
                    var slotBookings = await _context.Bookings
                        .Include(b => b.Subscription)
                            .ThenInclude(s => s.Member)
                        .Where(b => b.TimeSlotId == slot.Id
                            && b.BookingDate.Date == date.Date
                            && (b.Status == BookingStatus.Booked
                                || b.Status == BookingStatus.Attended))
                        .ToListAsync();

                    // Εύρεση μελών με ενεργή συνδρομή για αυτό το slot
                    var activeSubscriptions = await _context.Subscriptions
                        .Include(s => s.Member)
                        .Include(s => s.SubscriptionPlan)
                        .Include(s => s.Bookings)
                        .Where(s => s.IsActive
                            && s.SubscriptionPlan.GymProgramId == slot.GymProgramId
                            && s.SessionType == slot.SessionType)
                        .ToListAsync();

                    var availableMembers = activeSubscriptions.Select(s => new AvailableMember
                    {
                        SubscriptionId = s.Id,
                        MemberName = s.Member.Lastname + " " + s.Member.Firstname,
                        AlreadyBooked = slotBookings.Any(b => b.SubscriptionId == s.Id),
                        RemainingSessions = s.SubscriptionPlan.SessionsPerMonth - s.Bookings.Count(b =>
                            b.Status == BookingStatus.Booked ||
                            b.Status == BookingStatus.Attended ||
                            b.Status == BookingStatus.NoShow)
                    }).ToList();

                    slotInfos.Add(new SlotInfo
                    {
                        TimeSlot = slot,
                        Date = date,
                        BookedCount = slotBookings.Count,
                        AvailableCount = slot.Capacity - slotBookings.Count,
                        Bookings = slotBookings.Select(b => new BookingInfo
                        {
                            MemberName = b.Subscription.Member.Lastname + " " + b.Subscription.Member.Firstname,
                            Status = b.Status
                        }).ToList(),
                        AvailableMembers = availableMembers
                    });
                }

                Schedule[day] = slotInfos;
            }
        }
    }

    public class SlotInfo
    {
        public TimeSlot TimeSlot { get; set; } = default!;
        public DateTime Date { get; set; }
        public int BookedCount { get; set; }
        public int AvailableCount { get; set; }
        public bool IsFull => AvailableCount <= 0;
        public List<BookingInfo> Bookings { get; set; } = new();
        public List<AvailableMember> AvailableMembers { get; set; } = new();
    }

    public class BookingInfo
    {
        public string MemberName { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
    }

    public class AvailableMember
    {
        public int SubscriptionId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public bool AlreadyBooked { get; set; }
        public int RemainingSessions { get; set; }
    }
}