using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymApp.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }
        public int GymProgramId { get; set; }
        public int SessionsPerMonth { get; set; } // 4, 8, ή 12
        [Precision(10, 2)]
        public decimal Price { get; set; }

        // Navigation properties
        public GymProgram GymProgram { get; set; } = null!;
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
