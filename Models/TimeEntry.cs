// Models/TimeEntry.cs
using System;

namespace TimeTrackerApi.Models
{
    public class TimeEntry
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; } = string.Empty;

        // TEMP quick-fix for prod: use DateTime (no offset), we’ll treat as UTC in service
        public DateTime PunchInTime { get; set; }
        public DateTime? PunchOutTime { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}
