using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HexagonalArchitecture.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Handling {RequestName} with content: {RequestContent}",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(request));

            var response = await next();

            _logger.LogInformation(
                "Handled {RequestName} with response: {ResponseContent}",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(response));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling {RequestName} with content: {RequestContent}",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(request));
            throw;
        }
    }
}
