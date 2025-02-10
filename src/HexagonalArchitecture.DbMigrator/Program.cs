// See https://aka.ms/new-console-template for more information
using HexagonalArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var dbContext = host.Services.GetRequiredService<ApplicationDbContext>();

try
{
    logger.LogInformation("Starting database migration...");

    if (dbContext.Database.GetPendingMigrations().Any())
    {
        logger.LogInformation("Applying pending migrations...");
        dbContext.Database.Migrate();
        logger.LogInformation("Migrations applied successfully.");
    }
    else
    {
        logger.LogInformation("No pending migrations found.");
    }

    if (!dbContext.Database.CanConnect())
    {
        logger.LogInformation("Creating database...");
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Database created successfully.");
    }
    else
    {
        logger.LogInformation("Database already exists.");
    }

    logger.LogInformation("Database migration completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred while migrating the database.");
    throw;
}
