using Microsoft.AspNetCore.Identity;
using TimeTrackerApi.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
