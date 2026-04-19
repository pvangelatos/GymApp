using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Payments
{
    public class MemberPaymentsModel : PageModel
    {
        private readonly AppDbContext _context;

        public MemberPaymentsModel(AppDbContext context)
        {
            _context = context;
        }

        public Member Member { get; set; } = default!;
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalPaid { get; set; }
        public decimal TotalWithReceipt { get; set; }
        public decimal TotalWithoutReceipt { get; set; }

        public async Task<IActionResult> OnGetAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member == null) return NotFound();

            Member = member;

            Payments = await _context.Payments
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                        .ThenInclude(sp => sp.GymProgram)
                .Where(p => p.Subscription.MemberId == memberId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            TotalPaid = Payments.Sum(p => p.Amount);
            TotalWithReceipt = Payments.Where(p => p.HasReceipt).Sum(p => p.Amount);
            TotalWithoutReceipt = Payments.Where(p => !p.HasReceipt).Sum(p => p.Amount);

            return Page();
        }
    }
}
