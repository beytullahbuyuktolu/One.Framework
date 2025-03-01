#!/bin/sh

# Keycloak'un hazır olmasını bekle
sleep 30

# Admin token al
echo "Admin token alınıyor..."
ADMIN_TOKEN=$(curl -X POST http://keycloak:8080/realms/master/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin" \
  -d "password=admin" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" | jq -r '.access_token')

if [ -z "$ADMIN_TOKEN" ]; then
  echo "Admin token alınamadı!"
  exit 1
fi

echo "Admin token alındı."

# permission-sync-client için service account rollerini ayarla
echo "Service account rolleri ayarlanıyor..."

# Önce client ID'sini al
CLIENT_ID=$(curl -X GET http://keycloak:8080/admin/realms/hexagonal-architecture/clients \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" | jq -r '.[] | select(.clientId=="permission-sync-client") | .id')

if [ -z "$CLIENT_ID" ]; then
  echo "Client ID bulunamadı!"
  exit 1
fi

echo "Client ID: $CLIENT_ID"

# Service account user ID'sini al
SERVICE_ACCOUNT_USER_ID=$(curl -X GET http://keycloak:8080/admin/realms/hexagonal-architecture/clients/$CLIENT_ID/service-account-user \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" | jq -r '.id')

if [ -z "$SERVICE_ACCOUNT_USER_ID" ]; then
  echo "Service account user ID bulunamadı!"
  exit 1
fi

echo "Service Account User ID: $SERVICE_ACCOUNT_USER_ID"

# Realm-management client ID'sini al
REALM_MANAGEMENT_CLIENT_ID=$(curl -X GET http://keycloak:8080/admin/realms/hexagonal-architecture/clients \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: application/json" | jq -r '.[] | select(.clientId=="realm-management") | .id')

if [ -z "$REALM_MANAGEMENT_CLIENT_ID" ]; then
  echo "Realm-management client ID bulunamadı!"
  exit 1
fi

echo "Realm Management Client ID: $REALM_MANAGEMENT_CLIENT_ID"

# Gerekli rolleri ata
ROLES=("view-realm" "view-users" "view-clients" "view-authorization" "manage-realm" "manage-users" "manage-clients" "manage-authorization")

for ROLE in "${ROLES[@]}"; do
  # Rol ID'sini al
  ROLE_ID=$(curl -X GET http://keycloak:8080/admin/realms/hexagonal-architecture/clients/$REALM_MANAGEMENT_CLIENT_ID/roles/$ROLE \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" | jq -r '.id')

  if [ -n "$ROLE_ID" ]; then
    # Rolü service account kullanıcısına ata
    curl -X POST http://keycloak:8080/admin/realms/hexagonal-architecture/users/$SERVICE_ACCOUNT_USER_ID/role-mappings/clients/$REALM_MANAGEMENT_CLIENT_ID \
      -H "Authorization: Bearer $ADMIN_TOKEN" \
      -H "Content-Type: application/json" \
      -d "[{\"id\":\"$ROLE_ID\",\"name\":\"$ROLE\"}]"
    
    echo "Rol atandı: $ROLE"
  else
    echo "Rol bulunamadı: $ROLE"
  fi
done

echo "Keycloak yapılandırması tamamlandı!"
