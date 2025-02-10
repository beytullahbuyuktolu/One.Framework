using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace SharedKernel.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the request
        var request = await FormatRequest(context.Request);
        _logger.LogInformation($"HTTP Request Information:{Environment.NewLine}" +
                             $"Schema:{context.Request.Scheme} " +
                             $"Host: {context.Request.Host} " +
                             $"Path: {context.Request.Path} " +
                             $"QueryString: {context.Request.QueryString} " +
                             $"Request Body: {request}");

        // Copy a pointer to the original response body stream
        var originalBodyStream = context.Response.Body;

        // Create a new memory stream...
        using var responseBody = new MemoryStream();
        // ...and use it for the temporary response body
        context.Response.Body = responseBody;

        // Continue down the Middleware pipeline
        await _next(context);

        // Format the response from the server
        var response = await FormatResponse(context.Response);
        _logger.LogInformation($"HTTP Response Information:{Environment.NewLine}" +
                             $"Schema:{context.Request.Scheme} " +
                             $"Host: {context.Request.Host} " +
                             $"Path: {context.Request.Path} " +
                             $"QueryString: {context.Request.QueryString} " +
                             $"Response Body: {response}");

        // Copy the contents of the new memory stream (which contains the response) to the original stream
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        request.Body.Position = 0;

        return $"{body}";
    }

    private static async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return $"{text}";
    }
}
