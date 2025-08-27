namespace TimeTrackerApi.Models.Dtos
{
    public class SprintProgressDto
    {
        public Guid SprintId { get; set; }
        public int DoneStoryPoints { get; set; }
    }
}
