$keycloakUrl = "http://localhost:8080"
$realm = "hexagonal-architecture"
$clientId = "hexagonal-api"
$adminUsername = "admin"
$adminPassword = "admin"

# Get access token
$tokenResponse = Invoke-RestMethod -Method Post -Uri "$keycloakUrl/realms/master/protocol/openid-connect/token" -Body @{
    grant_type = "password"
    client_id = "admin-cli"
    username = $adminUsername
    password = $adminPassword
} -ContentType "application/x-www-form-urlencoded"

$accessToken = $tokenResponse.access_token
$headers = @{
    Authorization = "Bearer $accessToken"
    "Content-Type" = "application/json"
}

# Get realm ID
$realmInfo = Invoke-RestMethod -Method Get -Uri "$keycloakUrl/admin/realms/$realm" -Headers $headers
$realmId = $realmInfo.id

# Get client ID
$clients = Invoke-RestMethod -Method Get -Uri "$keycloakUrl/admin/realms/$realm/clients" -Headers $headers
$client = $clients | Where-Object { $_.clientId -eq $clientId }
$internalClientId = $client.id

# Create tenant ID mapper
$mapperData = @{
    name = "Tenant ID"
    protocol = "openid-connect"
    protocolMapper = "oidc-hardcoded-claim-mapper"
    config = @{
        "claim.name" = "client_tenant_id"
        "claim.value" = $realmId
        "jsonType.label" = "String"
        "id.token.claim" = "true"
        "access.token.claim" = "true"
        "userinfo.token.claim" = "true"
    }
} | ConvertTo-Json

# Delete existing mapper if exists
$existingMappers = Invoke-RestMethod -Method Get -Uri "$keycloakUrl/admin/realms/$realm/clients/$internalClientId/protocol-mappers/models" -Headers $headers
$existingMapper = $existingMappers | Where-Object { $_.name -eq "Tenant ID" }
if ($existingMapper) {
    Invoke-RestMethod -Method Delete -Uri "$keycloakUrl/admin/realms/$realm/clients/$internalClientId/protocol-mappers/models/$($existingMapper.id)" -Headers $headers
}

# Create new mapper
Invoke-RestMethod -Method Post -Uri "$keycloakUrl/admin/realms/$realm/clients/$internalClientId/protocol-mappers/models" -Headers $headers -Body $mapperData

Write-Host "Keycloak configuration completed successfully! Realm ID: $realmId"
