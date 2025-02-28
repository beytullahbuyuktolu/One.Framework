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

    public KeycloakSyncService(ILogger<KeycloakSyncService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _keycloakBaseUrl = configuration["Keycloak:Authority"]?.TrimEnd('/');
        _realm = configuration["Keycloak:Realm"];
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
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            foreach (var permission in missingPermissions)
            {
                try
                {
                    var createRoleUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/roles";

                    var roleData = new
                    {
                        name = permission,
                        description = $"Auto-generated permission: {permission}"
                    };

                    var response = await client.PostAsJsonAsync(createRoleUrl, roleData, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully created permission: {Permission}", permission);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning(
                            "Failed to create permission: {Permission}. Status: {Status}, Error: {Error}",
                            permission, response.StatusCode, errorContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating permission: {Permission}", permission);
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
            _logger.LogInformation("Getting roles from: {RolesUrl}", getRolesUrl);

            var response = await client.GetAsync(getRolesUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get roles. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to get roles: {errorContent}");
            }

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
        try
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"{_keycloakBaseUrl}/realms/master/protocol/openid-connect/token";

            _logger.LogInformation("Requesting admin token from: {TokenUrl}", tokenUrl);

            var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = _configuration["Keycloak:AdminUsername"],
                ["password"] = _configuration["Keycloak:AdminPassword"]
            });

            var response = await client.PostAsync(tokenUrl, tokenRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Admin token request failed. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to get admin token: {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken: cancellationToken);
            return tokenResponse?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining admin token");
            throw;
        }
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