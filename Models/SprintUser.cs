namespace TimeTrackerApi.Models
{
    public class SprintUser
    {
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        public Guid SprintId { get; set; }
        public Sprint Sprint { get; set; } = null!;
    }
}
