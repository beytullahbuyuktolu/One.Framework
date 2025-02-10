using IdentityService.Application.Common.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IdentityService.Api.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IIdentityDbContext _dbContext;

    public DatabaseHealthCheck(IIdentityDbContext dbContext)
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
