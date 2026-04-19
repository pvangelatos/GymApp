using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subscription Subscription { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription == null) return NotFound();

            Subscription = subscription;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subscription != null)
            {
                // Διαγραφή και των σχετικών Bookings
                _context.Bookings.RemoveRange(subscription.Bookings);
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { memberId = Subscription.MemberId });
        }
    }
}
