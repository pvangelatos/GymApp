using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Payments
{
    public class AllPaymentsModel : PageModel
    {
        private readonly AppDbContext _context;

        public AllPaymentsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Payment> Payments { get; set; } = new();
        public decimal TotalPaid { get; set; }
        public decimal TotalWithReceipt { get; set; }
        public decimal TotalWithoutReceipt { get; set; }

        public async Task OnGetAsync()
        {
            Payments = await _context.Payments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.Member)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                        .ThenInclude(sp => sp.GymProgram)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            TotalPaid = Payments.Sum(p => p.Amount);
            TotalWithReceipt = Payments.Where(p => p.HasReceipt).Sum(p => p.Amount);
            TotalWithoutReceipt = Payments.Where(p => !p.HasReceipt).Sum(p => p.Amount);
        }
    }
}
