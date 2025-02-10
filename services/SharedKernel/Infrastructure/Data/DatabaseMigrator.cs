using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Infrastructure.Data;

public class DatabaseMigrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(
        IServiceProvider serviceProvider,
        ILogger<DatabaseMigrator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MigrateAsync<TContext>(CancellationToken cancellationToken = default) where TContext : DbContext
    {
        try
        {
            _logger.LogInformation("Starting database migration for {ContextType}", typeof(TContext).Name);

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            await context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Database migration completed successfully for {ContextType}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database for {ContextType}", typeof(TContext).Name);
            throw;
        }
    }
}
