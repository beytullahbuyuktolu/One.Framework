using Microsoft.AspNetCore.Http;

namespace SharedKernel.Services;

public class TenantContextAccessor : ITenantContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
            return tenantId;
        }
    }

    public string? TenantName
    {
        get
        {
            var tenantName = _httpContextAccessor.HttpContext?.Items["TenantName"] as string;
            return tenantName;
        }
    }
}
