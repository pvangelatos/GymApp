using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [Precision(10, 2)]
        public decimal AmountPaid { get; set; }

        [Required]
        public SessionType SessionType { get; set; }

        // Navigation properties
        public Member Member { get; set; } = null!;
        public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
