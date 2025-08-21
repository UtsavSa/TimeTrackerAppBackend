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

        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<SprintUser> SprintUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for join table
            modelBuilder.Entity<SprintUser>()
                .HasKey(su => new { su.SprintId, su.UserId });

            modelBuilder.Entity<SprintUser>()
                .HasOne(su => su.Sprint)
                .WithMany(s => s.SprintUsers)
                .HasForeignKey(su => su.SprintId);

            modelBuilder.Entity<SprintUser>()
                .HasOne(su => su.User)
                .WithMany(u => u.SprintUsers)
                .HasForeignKey(su => su.UserId);

            //  Cascade delete: When a Sprint is deleted, its DashboardTasks are also deleted
            modelBuilder.Entity<DashboardTask>()
                .HasOne(t => t.Sprint)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.CreatedByUser)
                .WithMany() // No need for navigation from ApplicationUser unless needed
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete of user -> sprints

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