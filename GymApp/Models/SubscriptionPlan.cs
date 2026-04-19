using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class SubscriptionPlan
    {
        public int Id { get; set; }

        public int GymProgramId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Sessions")]
        [Range(1, 31, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                      ErrorMessageResourceName = "Range_Sessions")]
        public int SessionsPerMonth { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Price")]
        [Range(0.01, 10000, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                            ErrorMessageResourceName = "Range_Price")]
        [Precision(10, 2)]
        public decimal Price { get; set; }

        public GymProgram GymProgram { get; set; } = null!;
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}