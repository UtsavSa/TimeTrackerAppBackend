using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackerApi.Models
{
    public class DashboardTask
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int StoryPoints { get; set; }

        public double HoursNeeded { get; set; }

        public double HoursTaken { get; set; }

        public string Status { get; set; }

        [Required]
        public string? UserId { get; set; }

        public Guid? SprintId { get; set; }
        [ForeignKey("SprintId")]
        public Sprint? Sprint { get; set; }

    }
}
