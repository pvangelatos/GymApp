using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Pages.Subscriptions
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public Member Member { get; set; } = default!;
        public List<Subscription> Subscriptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);

            if (member == null)
                return NotFound();

            Member = member;

            Subscriptions = await _context.Subscriptions
                .Include(s => s.SubscriptionPlan)
                    .ThenInclude(sp => sp.GymProgram)
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            return Page();
        }
    }
}
