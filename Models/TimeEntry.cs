using System;

namespace TimeTrackerApi.Models
{
    public class TimeEntry
    {
        public Guid Id { get; set; }

        public string TaskName { get; set; } = string.Empty;

        public DateTime PunchInTime { get; set; }
        public DateTime? PunchOutTime { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
