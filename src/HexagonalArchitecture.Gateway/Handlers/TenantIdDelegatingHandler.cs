using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace HexagonalArchitecture.Gateway.Handlers;

public class TenantIdDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<TenantIdDelegatingHandler> _logger;

    public TenantIdDelegatingHandler(ILogger<TenantIdDelegatingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Headers.Authorization != null)
            {
                var token = request.Headers.Authorization.Parameter;
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                
                var tenantId = jwtToken.Claims.FirstOrDefault(c => c.Type == "client_tenant_id")?.Value;
                _logger.LogInformation($"Tenant ID from token: {tenantId}");

                if (!string.IsNullOrEmpty(tenantId))
                {
                    // Remove existing header if present
                    request.Headers.Remove("X-Tenant-ID");

                    // Add new header
                    request.Headers.TryAddWithoutValidation("X-Tenant-ID", tenantId);
                    _logger.LogInformation($"Added X-Tenant-ID header with value: {tenantId}");

                    // Log all headers
                    foreach (var header in request.Headers)
                    {
                        _logger.LogInformation($"{header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
                else
                {
                    _logger.LogWarning("No tenant ID found in token claims");
                }
            }
            else
            {
                _logger.LogWarning("No Authorization header found in request");
            }

            var response = await base.SendAsync(request, cancellationToken);
            _logger.LogInformation($"Response status code: {response.StatusCode}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TenantIdDelegatingHandler");
            throw;
        }
    }
}
