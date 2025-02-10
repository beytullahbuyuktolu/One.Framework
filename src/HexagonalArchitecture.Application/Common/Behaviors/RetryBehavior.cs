using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace HexagonalArchitecture.Application.Common.Behaviors;

public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;
    private readonly int _retryCount = 3;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(_retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(exception,
                        "Retry {RetryCount} of {RequestType}, due to: {ExceptionMessage}",
                        retryCount,
                        typeof(TRequest).Name,
                        exception.Message);
                })
            .ExecuteAsync(async () => await next());
    }
}
