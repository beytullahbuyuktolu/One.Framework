using AdministrationService.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdministrationService.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IAdministrationDbContext _dbContext;

    public DatabaseHealthCheck(IAdministrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            
            if (canConnect)
            {
                return HealthCheckResult.Healthy("Database connection is healthy.");
            }
            
            return HealthCheckResult.Unhealthy("Cannot connect to the database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check failed.", ex);
        }
    }
}
