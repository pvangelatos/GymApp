using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Reports
{
    public class AttendanceStatsModel : PageModel
    {
        private readonly AppDbContext _context;

        public AttendanceStatsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<MemberStats> MemberStatsList { get; set; } = new();
        public List<ProgramStats> ProgramStatsList { get; set; } = new();
        public int TotalBookings { get; set; }
        public int TotalAttended { get; set; }
        public int TotalNoShow { get; set; }
        public int TotalCancelled { get; set; }

        public async Task OnGetAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Subscription)
                    .ThenInclude(s => s.Member)
                .Include(b => b.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                        .ThenInclude(sp => sp.GymProgram)
                .ToListAsync();

            TotalBookings = bookings.Count;
            TotalAttended = bookings.Count(b => b.Status == BookingStatus.Attended);
            TotalNoShow = bookings.Count(b => b.Status == BookingStatus.NoShow);
            TotalCancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);

            // Στατιστικά ανά μέλος
            MemberStatsList = bookings
                .GroupBy(b => b.Subscription.Member)
                .Select(g => new MemberStats
                {
                    MemberName = g.Key.Lastname + " " + g.Key.Firstname,
                    TotalBookings = g.Count(),
                    Attended = g.Count(b => b.Status == BookingStatus.Attended),
                    NoShow = g.Count(b => b.Status == BookingStatus.NoShow),
                    Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled)
                })
                .OrderByDescending(m => m.Attended)
                .ToList();

            // Στατιστικά ανά πρόγραμμα
            ProgramStatsList = bookings
                .GroupBy(b => b.Subscription.SubscriptionPlan.GymProgram.Name)
                .Select(g => new ProgramStats
                {
                    ProgramName = g.Key,
                    TotalBookings = g.Count(),
                    Attended = g.Count(b => b.Status == BookingStatus.Attended),
                    NoShow = g.Count(b => b.Status == BookingStatus.NoShow),
                    Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled)
                })
                .OrderByDescending(p => p.Attended)
                .ToList();
        }
    }

    public class MemberStats
    {
        public string MemberName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int Attended { get; set; }
        public int NoShow { get; set; }
        public int Cancelled { get; set; }
        public double AttendanceRate => TotalBookings > 0
            ? Math.Round((double)Attended / TotalBookings * 100, 1)
            : 0;
    }

    public class ProgramStats
    {
        public string ProgramName { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int Attended { get; set; }
        public int NoShow { get; set; }
        public int Cancelled { get; set; }
        public double AttendanceRate => TotalBookings > 0
            ? Math.Round((double)Attended / TotalBookings * 100, 1)
            : 0;
    }
}
