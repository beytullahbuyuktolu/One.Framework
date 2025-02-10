using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace SharedKernel.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly int _maxRequests;
    private readonly TimeSpan _interval;

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, int maxRequests = 100, int intervalInSeconds = 60)
    {
        _next = next;
        _cache = cache;
        _maxRequests = maxRequests;
        _interval = TimeSpan.FromSeconds(intervalInSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GenerateClientKey(context);
        var clientStatistics = await GetClientStatisticsAsync(key);

        if (clientStatistics.NumberOfRequestsCompletedSuccessfully >= _maxRequests)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsJsonAsync(new { Message = "API rate limit exceeded." });
            return;
        }

        await UpdateClientStatisticsAsync(key, clientStatistics);
        await _next(context);
    }

    private static string GenerateClientKey(HttpContext context)
    {
        return $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";
    }

    private async Task<ClientStatistics> GetClientStatisticsAsync(string key)
    {
        var clientStatistics = _cache.Get<ClientStatistics>(key);

        if (clientStatistics != null)
            return clientStatistics;

        clientStatistics = new ClientStatistics
        {
            LastSuccessfulResponseTime = DateTime.UtcNow,
            NumberOfRequestsCompletedSuccessfully = 0
        };

        _cache.Set(key, clientStatistics, _interval);
        return await Task.FromResult(clientStatistics);
    }

    private async Task UpdateClientStatisticsAsync(string key, ClientStatistics clientStatistics)
    {
        clientStatistics.LastSuccessfulResponseTime = DateTime.UtcNow;
        clientStatistics.NumberOfRequestsCompletedSuccessfully++;

        _cache.Set(key, clientStatistics, _interval);
        await Task.CompletedTask;
    }
}

public class ClientStatistics
{
    public DateTime LastSuccessfulResponseTime { get; set; }
    public int NumberOfRequestsCompletedSuccessfully { get; set; }
}
