using HexagonalArchitecture.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HexagonalArchitecture.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity is ClaimsIdentity identity)
        {
            var tenantIdClaim = identity.Claims.FirstOrDefault(c => c.Type == "client_tenant_id");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out Guid tenantId))
            {
                return tenantId;
            }
        }

        throw new UnauthorizedAccessException("Tenant ID not found in token. Please make sure you are logged in and your token contains the client_tenant_id claim.");
    }
}
