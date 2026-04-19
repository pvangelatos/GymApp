using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.Payments
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Payment Payment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            Payment = payment;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index", new { subscriptionId = Payment.SubscriptionId });
        }
    }
}
