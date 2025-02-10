using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using HexagonalArchitecture.Application.Common.Interfaces;

namespace HexagonalArchitecture.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = $"{request.GetType().Name}_{request.CacheKey}";
        
        var cachedResponse = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation("Fetched from Cache -> '{Key}'", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedResponse);
        }

        var response = await next();

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.Expiration
        };

        var serializedResponse = JsonSerializer.Serialize(response);
        await _cache.SetStringAsync(cacheKey, serializedResponse, cacheOptions, cancellationToken);
        
        _logger.LogInformation("Added to Cache -> '{Key}'", cacheKey);

        return response;
    }
}
