using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SharedKernel.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items["TenantId"] = Guid.Parse(tenantId);
            context.Items["TenantName"] = context.User.FindFirst("tenant_name")?.Value;
        }
        else
        {
            // For endpoints that don't require authentication (like login),
            // try to get tenant from query string or header
            tenantId = context.Request.Query["tenantId"].ToString() ?? 
                      context.Request.Headers["X-Tenant-ID"].ToString();

            if (!string.IsNullOrEmpty(tenantId) && Guid.TryParse(tenantId, out var parsedTenantId))
            {
                context.Items["TenantId"] = parsedTenantId;
            }
        }

        await _next(context);
    }
}
