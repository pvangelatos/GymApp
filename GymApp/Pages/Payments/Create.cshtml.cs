using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Payments
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Payment Payment { get; set; } = new();

        public Subscription Subscription { get; set; } = default!;
        public SelectList PaymentMethodList { get; set; } = default!;
        public decimal RemainingAmount { get; set; }

        public async Task<IActionResult> OnGetAsync(int subscriptionId)
        {
            var subscription = await LoadSubscriptionAsync(subscriptionId);
            if (subscription == null) return NotFound();

            Subscription = subscription;
            Payment.SubscriptionId = subscriptionId;
            Payment.PaymentDate = DateTime.Today;
            Payment.Amount = await GetRemainingAmountAsync(subscription);

            LoadPaymentMethodList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Payment.Subscription");

            if (!ModelState.IsValid)
            {
                var subscription = await LoadSubscriptionAsync(Payment.SubscriptionId);
                Subscription = subscription!;
                LoadPaymentMethodList();
                return Page();
            }

            _context.Payments.Add(Payment);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index", new { subscriptionId = Payment.SubscriptionId });
        }

        private async Task<Subscription?> LoadSubscriptionAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .Include(s => s.Member)
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
        }

        private async Task<decimal> GetRemainingAmountAsync(Subscription subscription)
        {
            var totalPaid = await _context.Payments
                .Where(p => p.SubscriptionId == subscription.Id)
                .SumAsync(p => p.Amount);
            return subscription.AmountPaid - totalPaid;
        }

        private void LoadPaymentMethodList()
        {
            PaymentMethodList = new SelectList(new[]
            {
                new { Value = "Cash", Text = "💵 Μετρητά" },
                new { Value = "Card", Text = "💳 Κάρτα" },
                new { Value = "Transfer", Text = "🏦 Transfer" }
            }, "Value", "Text");
        }
    }
}
