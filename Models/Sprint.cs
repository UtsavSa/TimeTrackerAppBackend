using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrackerApi.Models
{
    public class Sprint
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        //The creator of the sprint

        public string? CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public ApplicationUser? CreatedByUser { get; set; }


        //Users in this sprint Many-to-many:
        public ICollection<SprintUser> SprintUsers { get; set; } = new List<SprintUser>();

        // Tasks in this sprint One-to-many: Tasks in this sprint
        public ICollection<DashboardTask> Tasks { get; set; } = new List<DashboardTask>();

     
        public int TotalStoryPoints { get; set; } = 0;
        public double TotalHoursTaken { get; set; } = 0.0;


    }
}
