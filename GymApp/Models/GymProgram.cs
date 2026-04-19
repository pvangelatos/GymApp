using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class GymProgram
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Program")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                          ErrorMessageResourceName = "StringLength_100")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                          ErrorMessageResourceName = "StringLength_500")]
        public string? Description { get; set; }

        [StringLength(200, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                          ErrorMessageResourceName = "StringLength_200")]
        public string? Schedule { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Trainer")]
        public int TrainerId { get; set; }

        public Trainer Trainer { get; set; } = null!;
        public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
    }
}
