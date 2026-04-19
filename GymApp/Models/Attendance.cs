namespace GymApp.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public DateTime Date { get; set; }

        // Navigation properties
        public Subscription Subscription { get; set; } = null!;
    }
}
