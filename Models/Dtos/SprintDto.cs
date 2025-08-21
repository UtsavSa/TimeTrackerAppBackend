namespace TimeTrackerApi.Models.Dtos
{
    public class SprintDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalStoryPoints { get; set; }
        public double TotalHoursTaken { get; set; }

        public List<string> UserEmails { get; set; } = new(); // Or UserIds/Usernames

        public string CreatedBy { get; set; } = ""; // ✅ Add this

    }
}
