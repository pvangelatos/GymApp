using GymApp.Data;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Services
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionExpirationService> _logger;

        public SubscriptionExpirationService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Expiration Service ξεκίνησε.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Υπολογισμός χρόνου μέχρι τα επόμενα 23:59
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddHours(23).AddMinutes(59);
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation($"Επόμενος έλεγχος συνδρομών: {nextRun:dd/MM/yyyy HH:mm}");

                await Task.Delay(delay, stoppingToken);

                await CheckAndDeactivateSubscriptionsAsync();
            }
        }

        private async Task CheckAndDeactivateSubscriptionsAsync()
        {
            _logger.LogInformation("Έλεγχος ληγμένων συνδρομών...");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Απενεργοποίηση βάσει EndDate
            var expiredSubscriptions = await db.Subscriptions
                .Where(s => s.IsActive && s.EndDate < DateTime.Today)
                .ToListAsync();

            foreach (var sub in expiredSubscriptions)
                sub.IsActive = false;

            // Απενεργοποίηση βάσει εξαντλημένων συνεδριών
            var exhaustedSubscriptions = await db.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Include(s => s.Bookings)
                .Where(s => s.IsActive)
                .ToListAsync();

            var toDeactivate = exhaustedSubscriptions.Where(s =>
                s.Bookings.Count(b =>
                    b.Status == BookingStatus.Attended ||
                    b.Status == BookingStatus.NoShow ||
                    b.Status == BookingStatus.Booked) >= s.SubscriptionPlan.SessionsPerMonth)
                .ToList();

            foreach (var sub in toDeactivate)
                sub.IsActive = false;

            var totalDeactivated = expiredSubscriptions.Count + toDeactivate.Count;
            if (totalDeactivated > 0)
            {
                await db.SaveChangesAsync();
                _logger.LogInformation($"Απενεργοποιήθηκαν {totalDeactivated} συνδρομές.");
            }
            else
            {
                _logger.LogInformation("Δεν βρέθηκαν ληγμένες συνδρομές.");
            }
        }
    }
}
