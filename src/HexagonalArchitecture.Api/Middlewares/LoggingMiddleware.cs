using System.Diagnostics;
using System.Text;

namespace HexagonalArchitecture.Api.Middlewares;
public class LoggingMiddleware : IMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        try
        {
            var requestInfo = await GetRequestInfo(context.Request);
            _logger.LogInformation("[Request] {Method} {Path} started - Headers: {@Headers}, Query: {Query}, Body: {Body}", context.Request.Method, context.Request.Path, context.Request.Headers, context.Request.QueryString.Value, requestInfo);

            var authInfo = new
            {
                IsAuthenticated = context.User?.Identity?.IsAuthenticated,
                UserName = context.User?.Identity?.Name,
                Claims = context.User?.Claims.Select(c => new { c.Type, c.Value })
            };
            _logger.LogInformation("[Auth] {@AuthInfo}", authInfo);

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await next(context);

            memoryStream.Position = 0;
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            sw.Stop();

            if (!context.Request.Path.Value.Contains("/swagger"))
                _logger.LogInformation("[Response] {Method} {Path} completed in {ElapsedMs}ms with Status: {StatusCode}, Content-Type: {ContentType}", context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode, context.Response.ContentType);
            else
                _logger.LogInformation("[Response] {Method} {Path} (Swagger UI) completed in {ElapsedMs}ms with Status: {StatusCode}", context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "[Error] {Method} {Path} failed in {ElapsedMs}ms", context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> GetRequestInfo(HttpRequest request)
    {
        if (!request.Body.CanRead)
            return string.Empty;
        try
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch
        {
            return string.Empty;
        }
    }
}