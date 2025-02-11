using HexagonalArchitecture.Domain.Configurations.Localization;
using HexagonalArchitecture.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace HexagonalArchitecture.Domain.Middlewares;
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly IStringLocalizer<OneResource> _localizer;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostingEnvironment _environment;

    public ExceptionHandlingMiddleware(IStringLocalizer<OneResource> localizer, ILogger<ExceptionHandlingMiddleware> logger, IHostingEnvironment environment)
    {
        _localizer = localizer;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred: {Message}", exception.Message);
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Status = GetStatusCode(exception),
            Error = new ErrorDetail
            {
                Code = GetErrorCode(exception),
                Message = _localizer[GetMessageKey(exception)].Value,
                Details = GetErrorDetails(exception)
            }
        };

        // Sadece development ortamında debug bilgilerini ekle
        if (_environment.IsDevelopment())
        {
            errorResponse.Debug = new DebugInfo
            {
                Exception = exception.GetType().Name,
                StackTrace = GetCleanStackTrace(exception),
                Source = GetExceptionSource(exception)
            };
        }

        context.Response.StatusCode = errorResponse.Status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }

    private string GetCleanStackTrace(Exception exception)
    {
        var stackTrace = exception.StackTrace ?? string.Empty;
        var firstLine = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault(x => x.Contains("HexagonalArchitecture"));

        return firstLine?.Trim() ?? string.Empty;
    }

    private string GetExceptionSource(Exception exception)
    {
        var source = exception.TargetSite?.DeclaringType?.FullName;
        return source?.Split('.').LastOrDefault() ?? string.Empty;
    }

    private int GetStatusCode(Exception exception) => exception switch
    {
        BusinessException ex => ex.StatusCode,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    private string GetErrorCode(Exception exception) => exception switch
    {
        BusinessException ex => ex is NotFoundException ? "NotFound" : "BusinessError",
        UnauthorizedAccessException => "Unauthorized",
        _ => "InternalError"
    };

    private string GetMessageKey(Exception exception) => exception switch
    {
        BusinessException ex => ex.MessageKey,
        UnauthorizedAccessException => "Error:Unauthorized",
        _ => "Error:InternalServer"
    };

    private object GetErrorDetails(Exception exception)
    {
        if (exception is BusinessException businessException)
        {
            return new
            {
                businessException.MessageKey,
                Timestamp = DateTime.UtcNow,
                Path = GetRequestPath()
            };
        }
        return null;
    }

    private string GetRequestPath()
    {
        var context = new HttpContextAccessor().HttpContext;
        return context?.Request.Path.Value ?? string.Empty;
    }
}