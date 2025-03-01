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
            var token = await GetAdminTokenAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiClient = await GetClientAsync(client, cancellationToken);
            if (apiClient == null) return;

            var missingPermissions = await GetMissingPermissionsAsync(cancellationToken);
            if (!missingPermissions.Any())
            {
                _logger.LogInformation("All permissions are already synced");
                return;
            }

            foreach (var permission in missingPermissions)
            {
                try
                {
                    // Create resource
                    var resource = await CreateResourceAsync(client, apiClient.Id, permission, cancellationToken);
                    if (resource == null) continue;

                    // Create empty policy for the permission
                    var policy = await CreateEmptyPolicyAsync(client, apiClient.Id, permission, cancellationToken);
                    if (policy == null) continue;

                    // Create permission with the empty policy
                    await CreatePermissionAsync(client, apiClient.Id, permission, resource.Id, policy.Id, cancellationToken);
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

    public async Task SyncRolePermissionsAsync(string roleName, IEnumerable<string> permissions, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await GetAdminTokenAsync(cancellationToken);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var apiClient = await GetClientAsync(client, cancellationToken);
            if (apiClient == null) return;

            // Get or create role
            var role = await GetOrCreateRoleAsync(client, roleName, cancellationToken);
            if (role == null) return;

            // Get existing role permissions
            var existingPermissions = await GetRolePermissionsAsync(client, role.Id, cancellationToken);
            var permissionsToAdd = permissions.Except(existingPermissions);

            foreach (var permission in permissionsToAdd)
            {
                await AssignPermissionToRoleAsync(client, apiClient.Id, role.Id, permission, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing role permissions for role {RoleName}", roleName);
        }
    }

    private async Task<KeycloakClient> GetClientAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var clientsUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients";
        var response = await client.GetAsync(clientsUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(cancellationToken);
        var apiClient = clients.FirstOrDefault(c => c.ClientId == _clientId);

        if (apiClient == null)
        {
            _logger.LogError("Client {ClientId} not found in Keycloak", _clientId);
            return null;
        }

        return apiClient;
    }

    private async Task<KeycloakResource> CreateResourceAsync(HttpClient client, string clientId, string permission, CancellationToken cancellationToken)
    {
        var resourceUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{clientId}/authz/resource-server/resource";
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
            scopes = new[] { new { name = "access" } }
        };

        var response = await client.PostAsJsonAsync(resourceUrl, resourceData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to create resource {Permission}. Error: {Error}", permission, error);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<KeycloakResource>(cancellationToken);
    }

    private async Task<KeycloakPolicy> CreateEmptyPolicyAsync(HttpClient client, string clientId, string permission, CancellationToken cancellationToken)
    {
        try 
        {
            // First try to get existing policy
            var policiesUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{clientId}/authz/resource-server/policy";
            var existingPolicies = await client.GetAsync($"{policiesUrl}?name={permission}_policy", cancellationToken);
            var policies = await existingPolicies.Content.ReadFromJsonAsync<List<KeycloakPolicy>>(cancellationToken);
            
            if (policies?.Any() == true)
            {
                _logger.LogInformation("Policy already exists for {Permission}", permission);
                return policies.First();
            }

            // Create new policy
            var policyUrl = $"{policiesUrl}/user";
            var policyData = new
            {
                name = $"{permission}_policy",
                description = $"Policy for {permission}",
                type = "user",
                logic = "POSITIVE",
                decisionStrategy = "UNANIMOUS",
                users = new string[] { }
            };

            _logger.LogInformation("Creating policy with data: {PolicyData}", System.Text.Json.JsonSerializer.Serialize(policyData));

            var response = await client.PostAsJsonAsync(policyUrl, policyData, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create policy {Permission}. Status: {Status}, Error: {Error}", 
                    permission, response.StatusCode, responseContent);
                return null;
            }

            _logger.LogInformation("Policy creation response: {Response}", responseContent);
            return await response.Content.ReadFromJsonAsync<KeycloakPolicy>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for {Permission}", permission);
            return null;
        }
    }

    private async Task CreatePermissionAsync(HttpClient client, string clientId, string permission, string resourceId, string policyId, CancellationToken cancellationToken)
    {
        var permissionUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{clientId}/authz/resource-server/permission/resource";
        var permissionData = new
        {
            name = $"{permission}_permission",
            description = $"Permission for {permission}",
            type = "resource",
            logic = "POSITIVE",
            decisionStrategy = "AFFIRMATIVE",
            resources = new[] { resourceId },
            policies = new[] { policyId }
        };

        var response = await client.PostAsJsonAsync(permissionUrl, permissionData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to create permission {Permission}. Error: {Error}", permission, error);
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

            var apiClient = await GetClientAsync(client, cancellationToken);
            if (apiClient == null) return Enumerable.Empty<string>();

            var resourcesUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{apiClient.Id}/authz/resource-server/resource";
            var response = await client.GetAsync(resourcesUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to get resources. Error: {Error}", error);
                return Enumerable.Empty<string>();
            }

            var resources = await response.Content.ReadFromJsonAsync<List<KeycloakResource>>(cancellationToken);
            return resources?.Select(r => r.Name) ?? Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting existing permissions");
            return Enumerable.Empty<string>();
        }
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var tokenUrl = $"{_keycloakBaseUrl}/realms/master/protocol/openid-connect/token";

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
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Admin token request failed. Error: {Error}", error);
                throw new HttpRequestException($"Failed to get admin token: {error}");
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

    private async Task<KeycloakRole> GetOrCreateRoleAsync(HttpClient client, string roleName, CancellationToken cancellationToken)
    {
        var rolesUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/roles";
        
        // Check if role exists
        var existingRole = await client.GetAsync($"{rolesUrl}/{roleName}", cancellationToken);
        if (existingRole.IsSuccessStatusCode)
        {
            return await existingRole.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken);
        }

        // Create new role
        var roleData = new
        {
            name = roleName,
            description = $"Auto-generated role: {roleName}",
            composite = false,
            clientRole = false
        };

        var response = await client.PostAsJsonAsync(rolesUrl, roleData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create role {RoleName}", roleName);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<KeycloakRole>(cancellationToken);
    }

    private async Task<IEnumerable<string>> GetRolePermissionsAsync(HttpClient client, string roleId, CancellationToken cancellationToken)
    {
        var permissionsUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/roles/{roleId}/permissions";
        var response = await client.GetAsync(permissionsUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get role permissions for role {RoleId}", roleId);
            return Enumerable.Empty<string>();
        }

        var permissions = await response.Content.ReadFromJsonAsync<List<KeycloakPermission>>(cancellationToken);
        return permissions?.Select(p => p.Name) ?? Enumerable.Empty<string>();
    }

    private async Task AssignPermissionToRoleAsync(HttpClient client, string clientId, string roleId, string permission, CancellationToken cancellationToken)
    {
        try
        {
            // Create resource and policy if not exists
            var resource = await CreateResourceAsync(client, clientId, permission, cancellationToken);
            if (resource == null) return;

            var policy = await CreateRolePolicyAsync(client, clientId, roleId, permission, cancellationToken);
            if (policy == null) return;

            // Create permission
            await CreatePermissionAsync(client, clientId, permission, resource.Id, policy.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission {Permission} to role {RoleId}", permission, roleId);
        }
    }

    private async Task<KeycloakPolicy> CreateRolePolicyAsync(HttpClient client, string clientId, string roleId, string permission, CancellationToken cancellationToken)
    {
        var policyUrl = $"{_keycloakBaseUrl}/admin/realms/{_realm}/clients/{clientId}/authz/resource-server/policy/role";
        var policyData = new
        {
            name = $"{permission}_role_policy",
            description = $"Role policy for {permission}",
            type = "role",
            logic = "POSITIVE",
            decisionStrategy = "UNANIMOUS",
            roles = new[] { new { id = roleId } }
        };

        var response = await client.PostAsJsonAsync(policyUrl, policyData, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to create role policy for permission {Permission}", permission);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<KeycloakPolicy>(cancellationToken);
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

    private class KeycloakPolicy
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    private class KeycloakRole
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    private class KeycloakPermission
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}