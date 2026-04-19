using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Attendances
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null)
                return NotFound();

            // Έλεγχος διαθέσιμων συνεδριών
            var attendanceCount = await _context.Attendances
                .CountAsync(a => a.SubscriptionId == subscriptionId);

            if (attendanceCount >= subscription.SubscriptionPlan.SessionsPerMonth)
                return RedirectToPage("Index", new { subscriptionId });

            var attendance = new Attendance
            {
                SubscriptionId = subscriptionId,
                Date = DateTime.Today
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { subscriptionId });
        }
    }
}
