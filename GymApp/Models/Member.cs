namespace GymApp.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime DateJoined { get; set; }
        public bool IsActive { get; set; } = true;


        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
