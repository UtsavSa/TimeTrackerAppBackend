using Microsoft.AspNetCore.Identity;
using TimeTrackerApi.Models;

public class ApplicationUser : IdentityUser
{
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

    // New: Sprints this user is part of
    public ICollection<SprintUser> SprintUsers { get; set; } = new List<SprintUser>();
}
