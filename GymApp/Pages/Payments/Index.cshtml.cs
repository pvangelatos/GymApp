using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Payments
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public Subscription Subscription { get; set; } = default!;
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalPaid { get; set; }
        public decimal TotalWithReceipt { get; set; }

        public async Task<IActionResult> OnGetAsync(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null) return NotFound();

            Subscription = subscription;

            Payments = await _context.Payments
                .Where(p => p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            TotalPaid = Payments.Sum(p => p.Amount);
            TotalWithReceipt = Payments
                .Where(p => p.HasReceipt)
                .Sum(p => p.Amount);

            return Page();
        }
    }
}
