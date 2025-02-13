using HexagonalArchitecture.Domain.Configurations.KeyCloak.Interfaces;

namespace HexagonalArchitecture.Api.Extensions;
public class PermissionSyncHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSyncHostedService> _logger;
    private readonly IConfiguration _configuration;

    public PermissionSyncHostedService(IServiceProvider serviceProvider, ILogger<PermissionSyncHostedService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IKeycloakSyncService>();
                await syncService.SyncPermissionsAsync(stoppingToken);
                var interval = _configuration.GetValue("PermissionSync:IntervalHours", 1);
                await Task.Delay(TimeSpan.FromHours(interval), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during permission sync");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}