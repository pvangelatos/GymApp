namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public ICollection<GymProgram> GymPrograms { get; set; } = new List<GymProgram>();

    }
}
