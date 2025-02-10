using AdministrationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SharedKernel.MultiTenancy;

namespace AdministrationService.DbMigrator;

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
            Log.Information("Starting AdministrationService DbMigrator...");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?.Replace("{projectName}", "administration");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("Connection string 'DefaultConnection' not found.");
            }

            var tenantInfo = new DefaultTenantInfo
            {
                ConnectionString = connectionString
            };

            // Migrate AdministrationService DbContext
            Log.Information("Migrating AdministrationDbContext...");
            var contextOptions = new DbContextOptionsBuilder<AdministrationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using (var context = new AdministrationDbContext(contextOptions, tenantInfo))
            {
                await context.Database.MigrateAsync();
            }

            Log.Information("AdministrationService database migrations completed successfully.");
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
