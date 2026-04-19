using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        public int GymProgramId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public int Capacity { get; set; } = 5;

        [Required]
        public SessionType SessionType { get; set; }

        // Navigation properties
        public GymProgram GymProgram { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
