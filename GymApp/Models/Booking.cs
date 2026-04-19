using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public enum BookingStatus
    {
        Booked,
        Attended,
        Cancelled,
        NoShow
    }

    public class Booking
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }

        public int TimeSlotId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Booked;

        public DateTime? CancelledAt { get; set; }

        // Navigation properties
        public Subscription Subscription { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
    }
}
