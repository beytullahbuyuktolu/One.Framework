using HexagonalArchitecture.Domain.Configurations.KeyCloak.Interfaces;
using HexagonalArchitecture.Domain.Permissions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HexagonalArchitecture.Domain.Configurations.KeyCloak.Services;

public class KeycloakSyncService : IKeycloakSyncService
{
    private readonly ILogger<KeycloakSyncService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly string _keycloakBaseUrl;
    private readonly string _realm;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakSyncService(ILogger<KeycloakSyncService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;

        _keycloakBaseUrl = _configuration["Keycloak:BaseUrl"];
        _realm = _configuration["Keycloak:Realm"];
        _clientId = _configuration["Keycloak:ClientId"];
        _clientSecret = _configuration["Keycloak:ClientSecret"];
    }

    public async Task SyncPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var missingPermissions = await GetMissingPermissionsAsync(cancellationToken);

            if (!missingPermissions.Any())
            {
                _logger.LogInformation("All permissions are already synced");
                return;
            }

            var token = await GetAdminTokenAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            foreach (var permission in missingPermissions)
            {
                try
                {
                    // Keycloak'ta rol oluştur
                    var createRoleUrl =
                        $"{_keycloakBaseUrl}/admin/realms/{_realm}/roles";

                    var roleData = new
                    {
                        name = permission,
                        description = $"Auto-generated permission: {permission}"
                    };

                    var response = await client.PostAsJsonAsync(createRoleUrl, roleData, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Successfully created permission: {Permission}", permission);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Failed to create permission: {Permission}. Status: {Status}",
                            permission, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error creating permission: {Permission}", permission);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during permission sync");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetMissingPermissionsAsync(CancellationToken cancellationToken = default)
    {

        var existingPermissions = await GetExistingPermissionsAsync(cancellationToken);
        var allPermissions = OnePermissions.GetAllPermissions();
        return allPermissions.Except(existingPermissions);
    }

    public async Task<IEnumerable<string>> GetExistingPermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAdminTokenAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var getRolesUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/roles";

            var response = await client.GetAsync(getRolesUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRole>>(cancellationToken: cancellationToken);

            return roles?.Select(r => r.Name) ?? Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing permissions from Keycloak");
            throw;
        }
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var tokenUrl = $"{_keycloakBaseUrl}/realms/master/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = "admin-cli",
            ["username"] = _configuration["Keycloak:AdminUsername"],
            ["password"] = _configuration["Keycloak:AdminPassword"]
        });

        var response = await client.PostAsync(tokenUrl, tokenRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);

        return tokenResponse?.AccessToken;
    }

    private class KeycloakRole
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}