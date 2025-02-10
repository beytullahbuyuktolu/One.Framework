using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SharedKernel.MultiTenancy
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantService _tenantService;

        public TenantMiddleware(RequestDelegate next, ITenantService tenantService)
        {
            _next = next;
            _tenantService = tenantService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tenantId = context.Request.Headers["X-TenantId"].ToString();
            
            if (!string.IsNullOrEmpty(tenantId))
            {
                // In a real application, you would look up the tenant in a database
                var tenant = new TenantInfo { Id = System.Guid.Parse(tenantId) };
                _tenantService.SetCurrentTenant(tenant);
            }

            await _next(context);
        }
    }
}
