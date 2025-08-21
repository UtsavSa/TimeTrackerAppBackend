namespace TimeTrackerApi.Models.Dtos
{
    public class CreateSprintDto
    {
        public string Name { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
