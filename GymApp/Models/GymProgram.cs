namespace GymApp.Models
{
    public class GymProgram
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Schedule { get; set; }
        public int TrainerId { get; set; }

        public Trainer Trainer { get; set; } = null!;
        public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();


    }
}
