using GymApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymApp.Pages.Attendances
{
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);

            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();

                return RedirectToPage("Index", new { subscriptionId = attendance.SubscriptionId });
            }

            return RedirectToPage("/Subscriptions/Index");
        }
    }
}