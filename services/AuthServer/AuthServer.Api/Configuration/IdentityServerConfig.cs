using Duende.IdentityServer.Models;
using System.Collections.Generic;

namespace AuthServer.Api.Configuration
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                    Name = "role",
                    UserClaims = new List<string> { "role" }
                }
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("api1", "My API"),
                new ApiScope("administration", "Administration API"),
                new ApiScope("identity", "Identity API")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("api1", "My API")
                {
                    Scopes = { "api1" }
                },
                new ApiResource("administration", "Administration API")
                {
                    Scopes = { "administration" }
                },
                new ApiResource("identity", "Identity API")
                {
                    Scopes = { "identity" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                // Machine to machine client
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "api1", "administration", "identity" }
                },

                // Interactive ASP.NET Core Web App
                new Client
                {
                    ClientId = "web",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "profile",
                        "email",
                        "api1",
                        "administration",
                        "identity",
                        "role"
                    },
                    RequirePkce = true,
                    AllowOfflineAccess = true
                }
            };
    }
}
