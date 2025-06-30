using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Data
{
    public class TimeTrackerContext : IdentityDbContext<ApplicationUser>
    {
        public TimeTrackerContext(DbContextOptions<TimeTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<DashboardTask> DashboardTasks { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}



//-----


//That: DbContext is what gives your context access to:

//.Add()

//.Update()

//.Remove()

//.RemoveRange() is the bulk deletion version of .Remove().

//.SaveChangesAsync()

//and many more!