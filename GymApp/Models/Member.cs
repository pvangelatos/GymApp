using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Firstname")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                        ErrorMessageResourceName = "StringLength_50")]
        public string Firstname { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Lastname")]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                        ErrorMessageResourceName = "StringLength_50")]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                  ErrorMessageResourceName = "Required_Email")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                      ErrorMessageResourceName = "InvalidEmail")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                          ErrorMessageResourceName = "StringLength_100")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
               ErrorMessageResourceName = "InvalidPhone")]
        [StringLength(20)]
        public string? Phone { get; set; }
        public DateTime DateJoined { get; set; }
        public bool IsActive { get; set; } = true;


        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
