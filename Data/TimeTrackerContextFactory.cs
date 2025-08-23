// Data/TimeTrackerContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TimeTrackerApi.Data;

public class TimeTrackerContextFactory : IDesignTimeDbContextFactory<TimeTrackerContext>
{
    public TimeTrackerContext CreateDbContext(string[] args)
    {
        var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var cfg = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Use pg for design-time (provider choice matters for migration SQL)
        var cs = cfg.GetConnectionString("DefaultConnection")
                 ?? System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                 ?? "Host=localhost;Port=5432;Database=dummy;Username=dummy;Password=dummy;Ssl Mode=Require;Trust Server Certificate=true";

        var options = new DbContextOptionsBuilder<TimeTrackerContext>()
            .UseNpgsql(cs)  // <— IMPORTANT: Npgsql provider
            .Options;

        return new TimeTrackerContext(options);
    }
}
