using System;

namespace TimeTrackerApi.Models
{

    public class TimeEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? TaskName { get; set; }
        public DateTime PunchInTime { get; set; }
        public DateTime? PunchOutTime { get; set; }
        public string? UserId { get; set; }
    }

}






















