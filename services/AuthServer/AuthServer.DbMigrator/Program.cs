using AuthServer.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace AuthServer.DbMigrator;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Duende", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting AuthServer DbMigrator...");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?.Replace("{projectName}", "authserver");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found.");
            }

            // Migrate AuthServer DbContext
            Log.Information("Migrating AuthServerDbContext...");
            var authServerContextOptions = new DbContextOptionsBuilder<AuthServerDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var context = new AuthServerDbContext(authServerContextOptions))
            {
                await context.Database.MigrateAsync();
            }

            Log.Information("AuthServer database migrations completed successfully.");
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
