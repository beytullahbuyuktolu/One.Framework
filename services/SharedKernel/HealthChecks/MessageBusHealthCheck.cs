using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using SharedKernel.Messaging.RabbitMQ;

namespace SharedKernel.HealthChecks;

public class MessageBusHealthCheck : IHealthCheck
{
    private readonly RabbitMQSettings _settings;

    public MessageBusHealthCheck(RabbitMQSettings settings)
    {
        _settings = settings;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection is healthy."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection failed.", ex));
        }
    }
}
