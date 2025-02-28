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

    public KeycloakSyncService(ILogger<KeycloakSyncService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _keycloakBaseUrl = configuration["Keycloak:Authority"]?.TrimEnd('/');
        _realm = configuration["Keycloak:Realm"];
        _clientId = configuration["Keycloak:ClientId"];
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

            // Get client ID from Keycloak
            var clientsUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients";
            var response = await client.GetAsync(clientsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(cancellationToken);
            var apiClient = clients.FirstOrDefault(c => c.ClientId == _clientId);

            if (apiClient == null)
            {
                _logger.LogError("Client {ClientId} not found in Keycloak", _clientId);
                return;
            }

            foreach (var permission in missingPermissions)
            {
                try
                {
                    // Create resource
                    var resourceUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{apiClient.Id}/authz/resource-server/resource";
                    var resourceData = new
                    {
                        name = permission,
                        displayName = permission,
                        type = "urn:hexagonal-api:resources:permission",
                        ownerManagedAccess = false,
                        attributes = new Dictionary<string, List<string>>
                        {
                            ["permission_type"] = new List<string> { "application_permission" }
                        },
                        scopes = new[]
                        {
                            new { name = "access" }
                        }
                    };

                    var resourceResponse = await client.PostAsJsonAsync(resourceUrl, resourceData, cancellationToken);
                    
                    if (resourceResponse.IsSuccessStatusCode)
                    {
                        var resource = await resourceResponse.Content.ReadFromJsonAsync<KeycloakResource>(cancellationToken);

                        // Create permission
                        var policyUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{apiClient.Id}/authz/resource-server/permission/resource";
                        var policyData = new
                        {
                            name = $"{permission}_permission",
                            description = $"Permission for {permission}",
                            type = "resource",
                            logic = "POSITIVE",
                            decisionStrategy = "UNANIMOUS",
                            resources = new[] { resource.Id },
                            policies = Array.Empty<string>()
                        };

                        var policyResponse = await client.PostAsJsonAsync(policyUrl, policyData, cancellationToken);

                        if (policyResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Successfully created permission: {Permission}", permission);
                        }
                        else
                        {
                            var errorContent = await policyResponse.Content.ReadAsStringAsync(cancellationToken);
                            _logger.LogWarning(
                                "Failed to create permission policy: {Permission}. Status: {Status}, Error: {Error}",
                                permission, policyResponse.StatusCode, errorContent);
                        }
                    }
                    else
                    {
                        var errorContent = await resourceResponse.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning(
                            "Failed to create permission resource: {Permission}. Status: {Status}, Error: {Error}",
                            permission, resourceResponse.StatusCode, errorContent);
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

            // Get client ID first
            var clientsUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients";
            var response = await client.GetAsync(clientsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(cancellationToken);
            var apiClient = clients.FirstOrDefault(c => c.ClientId == _clientId);

            if (apiClient == null)
            {
                _logger.LogError("Client {ClientId} not found in Keycloak", _clientId);
                return Enumerable.Empty<string>();
            }

            var resourcesUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{apiClient.Id}/authz/resource-server/resource";
            _logger.LogInformation("Getting resources from: {ResourcesUrl}", resourcesUrl);

            var resourcesResponse = await client.GetAsync(resourcesUrl, cancellationToken);
            
            if (!resourcesResponse.IsSuccessStatusCode)
            {
                var errorContent = await resourcesResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get resources. Status: {Status}, Error: {Error}", 
                    resourcesResponse.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to get resources: {errorContent}");
            }

            var resources = await resourcesResponse.Content.ReadFromJsonAsync<List<KeycloakResource>>(cancellationToken);
            return resources?.Select(r => r.Name) ?? Enumerable.Empty<string>();
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

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(cancellationToken);
            return tokenResponse?.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obtaining admin token");
            throw;
        }
    }

    private class KeycloakClient
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("clientId")]
        public string ClientId { get; set; }
    }

    private class KeycloakResource
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }
}