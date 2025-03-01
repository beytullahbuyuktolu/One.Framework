using HexagonalArchitecture.Domain.Configurations.KeyCloak.Interfaces;
using HexagonalArchitecture.Domain.Permissions;
using System.Text.Json.Serialization;

namespace HexagonalArchitecture.Api.Extensions;

public class PermissionSyncHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PermissionSyncHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _keycloakBaseUrl;

    public PermissionSyncHostedService(
        IServiceProvider serviceProvider, 
        ILogger<PermissionSyncHostedService> logger, 
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _keycloakBaseUrl = configuration["Keycloak:Authority"].TrimEnd('/');
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IKeycloakSyncService>();
                
                var token = await GetServiceAccountTokenAsync(stoppingToken);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to obtain service account token");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                _logger.LogInformation("Successfully obtained service account token");
                
                // Sync permissions
                await syncService.SyncPermissionsAsync(stoppingToken);
                
                // Sync role-based permissions
                await SyncRoleBasedPermissionsAsync(syncService, stoppingToken);
                
                var interval = _configuration.GetValue("PermissionSync:IntervalHours", 1);
                await Task.Delay(TimeSpan.FromHours(interval), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during permission sync");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task<string> GetServiceAccountTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"{_keycloakBaseUrl}/realms/hexagonal-architecture/protocol/openid-connect/token";

            _logger.LogInformation("Requesting token from: {TokenUrl}", tokenUrl);

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "permission-sync-client",
                ["client_secret"] = "permission-sync-secret"
            });

            var response = await client.PostAsync(tokenUrl, tokenRequest, cancellationToken);
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Token response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token request failed. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, responseContent);
                return null;
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<KeycloakTokenResponse>(responseContent, options);
            
            if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
            {
                _logger.LogError("Token response does not contain access_token");
                return null;
            }

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining service account token");
            return null;
        }
    }

    private async Task SyncRoleBasedPermissionsAsync(IKeycloakSyncService syncService, CancellationToken cancellationToken)
    {
        try
        {
            // Standart rolleri ve izinleri senkronize et
            var standardRoles = new Dictionary<string, IEnumerable<string>>
            {
                {
                    "admin",
                    new[]
                    {
                        StandardPermissions.Users.Default,
                        StandardPermissions.Users.Create,
                        StandardPermissions.Users.Read,
                        StandardPermissions.Users.Update,
                        StandardPermissions.Users.Delete,
                        StandardPermissions.Users.ManagePermissions,
                        StandardPermissions.Roles.Default,
                        StandardPermissions.Roles.Create,
                        StandardPermissions.Roles.Read,
                        StandardPermissions.Roles.Update,
                        StandardPermissions.Roles.Delete,
                        StandardPermissions.Roles.ManagePermissions,
                        StandardPermissions.Tenants.Default,
                        StandardPermissions.Tenants.Create,
                        StandardPermissions.Tenants.Read,
                        StandardPermissions.Tenants.Update,
                        StandardPermissions.Tenants.Delete,
                        StandardPermissions.Tenants.ManageFeatures
                    }
                },
                {
                    "user",
                    new[]
                    {
                        StandardPermissions.Users.Read,
                        StandardPermissions.Roles.Read
                    }
                }
            };

            foreach (var role in standardRoles)
            {
                await syncService.SyncRolePermissionsAsync(role.Key, role.Value, cancellationToken);
                _logger.LogInformation("Synchronized permissions for role: {Role}", role.Key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during role-based permission sync");
        }
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}