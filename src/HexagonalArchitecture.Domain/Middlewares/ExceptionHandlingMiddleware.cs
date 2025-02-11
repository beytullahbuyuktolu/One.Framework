using HexagonalArchitecture.Domain.Configurations.Localization;
using HexagonalArchitecture.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HexagonalArchitecture.Domain.Middlewares;
public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly IStringLocalizer<OneResource> _localizer;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(IStringLocalizer<OneResource> localizer, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "Common:UnexpectedError");
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, int statusCode, string messageKey)
    {
        var response = new
        {
            Status = statusCode,
            Message = _localizer[messageKey]
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}