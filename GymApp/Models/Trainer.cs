using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
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

        [EmailAddress(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                      ErrorMessageResourceName = "InvalidEmail")]
        [StringLength(100, ErrorMessageResourceType = typeof(Resources.ValidationMessages),
                          ErrorMessageResourceName = "StringLength_100")]
        public string? Email { get; set; }

        [Phone(ErrorMessageResourceType = typeof(Resources.ValidationMessages),
               ErrorMessageResourceName = "InvalidPhone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        public ICollection<GymProgram> GymPrograms { get; set; } = new List<GymProgram>();
    }
}
