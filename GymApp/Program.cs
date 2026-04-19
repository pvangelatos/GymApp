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

            app.Run();
        }
    }
}
