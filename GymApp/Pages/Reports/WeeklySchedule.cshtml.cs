using GymApp.Data;
using GymApp.Models;
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
                    var bookedCount = await _context.Bookings
                        .CountAsync(b => b.TimeSlotId == slot.Id
                            && b.BookingDate.Date == date.Date
                            && (b.Status == BookingStatus.Booked
                                || b.Status == BookingStatus.Attended));

                    slotInfos.Add(new SlotInfo
                    {
                        TimeSlot = slot,
                        Date = date,
                        BookedCount = bookedCount,
                        AvailableCount = slot.Capacity - bookedCount
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
    }
}