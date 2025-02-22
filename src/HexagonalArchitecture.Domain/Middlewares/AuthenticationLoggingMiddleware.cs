using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HexagonalArchitecture.Domain.Middlewares;

public class AuthenticationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationLoggingMiddleware> _logger;

    public AuthenticationLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation(
            "Auth Details - IsAuthenticated: {IsAuthenticated}, User: {User}, Claims: {Claims}",
            context.User?.Identity?.IsAuthenticated,
            context.User?.Identity?.Name,
            string.Join(", ", context.User?.Claims.Select(c => $"{c.Type}: {c.Value}") ?? Array.Empty<string>())
        );

        await _next(context);
    }
}