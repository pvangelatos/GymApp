using GymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GymApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration
                    .GetConnectionString("DefaultConnection")));

            var cultureInfo = new System.Globalization.CultureInfo("el-GR");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            builder.Services.AddLocalization(options =>
                options.ResourcesPath = "Resources");

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "el-GR", "en-US" };
                options.SetDefaultCulture("el-GR")
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
            });

            builder.Services.AddHostedService<GymApp.Services.SubscriptionExpirationService>();

            var app = builder.Build();

            app.UseRequestLocalization();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();


            // Αυτόματη απενεργοποίηση ληγμένων συνδρομών
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Απενεργοποίηση βάσει EndDate
                var expiredSubscriptions = db.Subscriptions
                    .Where(s => s.IsActive && s.EndDate < DateTime.Today)
                    .ToList();

                foreach (var sub in expiredSubscriptions)
                    sub.IsActive = false;

                if (expiredSubscriptions.Any())
                    db.SaveChanges();

                // Απενεργοποίηση βάσει εξαντλημένων συνεδριών
                var exhaustedSubscriptions = db.Subscriptions
                    .Include(s => s.SubscriptionPlan)
                    .Include(s => s.Bookings)
                    .Where(s => s.IsActive)
                    .ToList()
                    .Where(s => s.Bookings.Count(b =>
                        b.Status == GymApp.Models.BookingStatus.Attended ||
                        b.Status == GymApp.Models.BookingStatus.NoShow ||
                        b.Status == GymApp.Models.BookingStatus.Booked) >= s.SubscriptionPlan.SessionsPerMonth)
                    .ToList();

                foreach (var sub in exhaustedSubscriptions)
                    sub.IsActive = false;

                if (exhaustedSubscriptions.Any())
                    db.SaveChanges();
            }

            app.Run();
            
        }
    }
}
