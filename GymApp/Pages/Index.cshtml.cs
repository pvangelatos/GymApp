using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int TotalPrograms { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ExpiredSubscriptions { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<(string ProgramName, int Count)> TopPrograms { get; set; } = new();
        public int ExpiringSubscriptions { get; set; }
        public List<Subscription> ExpiringSoonList { get; set; } = new();

        public CapacityInfo MorningCapacity { get; set; } = new();
        public CapacityInfo AfternoonCapacity { get; set; } = new();

        public async Task OnGetAsync()
        {
            var reformer = await _context.GymPrograms
                .FirstOrDefaultAsync(g => g.Name.Contains("Reformer"));

            if (reformer != null)
            {
                MorningCapacity = await GetCapacityInfoAsync(reformer.Id, SessionType.Morning);
                AfternoonCapacity = await GetCapacityInfoAsync(reformer.Id, SessionType.Afternoon);
            }

            ViewData["ExpiringCount"] = ExpiringSubscriptions;
            TotalMembers = await _context.Members.CountAsync();
            ActiveMembers = await _context.Members.CountAsync(m => m.IsActive);
            TotalPrograms = await _context.GymPrograms.CountAsync();

            ActiveSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.IsActive);

            ExpiredSubscriptions = await _context.Subscriptions
                .CountAsync(s => !s.IsActive);

            var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            MonthlyRevenue = await _context.Subscriptions
                .Where(s => s.StartDate >= firstDayOfMonth)
                .SumAsync(s => s.AmountPaid);

            TopPrograms = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Where(s => s.IsActive)
                .GroupBy(s => s.SubscriptionPlan.GymProgram.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(g => (g.Name, g.Count)).ToList());

            var sevenDaysFromNow = DateTime.Today.AddDays(7);
            ExpiringSubscriptions = await _context.Subscriptions
                .CountAsync(s => s.IsActive && s.EndDate <= sevenDaysFromNow);

            ExpiringSoonList = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Where(s => s.IsActive && s.EndDate <= sevenDaysFromNow)
                .OrderBy(s => s.EndDate)
                .ToListAsync();
        }

        private async Task<CapacityInfo> GetCapacityInfoAsync(int gymProgramId, SessionType sessionType)
        {
            // Συνολικές θέσεις/εβδομάδα
            var totalSlots = await _context.TimeSlots
                .Where(t => t.GymProgramId == gymProgramId && t.SessionType == sessionType)
                .SumAsync(t => t.Capacity);

            // Δεσμευμένες θέσεις από ενεργές συνδρομές
            var occupiedSlots = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.IsActive
                    && s.SessionType == sessionType
                    && s.SubscriptionPlan.GymProgramId == gymProgramId)
                .SumAsync(s => s.SubscriptionPlan.SessionsPerMonth / 4);

            var availableSlots = totalSlots - occupiedSlots;

            return new CapacityInfo
            {
                TotalSlots = totalSlots,
                OccupiedSlots = occupiedSlots,
                AvailableSlots = availableSlots,
                AvailableFor4 = availableSlots,
                AvailableFor8 = availableSlots / 2,
                AvailableFor12 = availableSlots / 3
            };
        }
    }
    public class CapacityInfo
    {
        public int TotalSlots { get; set; }
        public int OccupiedSlots { get; set; }
        public int AvailableSlots { get; set; }
        public int AvailableFor4 { get; set; }
        public int AvailableFor8 { get; set; }
        public int AvailableFor12 { get; set; }
    }
}
