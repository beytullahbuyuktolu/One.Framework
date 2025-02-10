using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace IdentityService.DbMigrator;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting IdentityService DbMigrator...");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?.Replace("{projectName}", "identity");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found.");
            }

            // Migrate IdentityService DbContext
            Log.Information("Migrating IdentityDbContext...");
            var contextOptions = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var context = new IdentityDbContext(contextOptions))
            {
                await context.Database.MigrateAsync();
            }

            Log.Information("IdentityService database migrations completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while migrating the database.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
