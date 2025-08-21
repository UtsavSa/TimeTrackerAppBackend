namespace TimeTrackerApi.Models.Dtos
{
    public class CreateTaskDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int StoryPoints { get; set; }
        public double HoursNeeded { get; set; }
        public double HoursTaken { get; set; }
        public string Status { get; set; } = "";

        public Guid? SprintId { get; set; } // ✅ New

    }
}
