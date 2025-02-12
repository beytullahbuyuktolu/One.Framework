using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace HexagonalArchitecture.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["IdentityServer:Authority"];
            options.Audience = configuration["IdentityServer:Audience"];
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = false,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = configuration["IdentityServer:Authority"],
                ValidAudience = configuration["IdentityServer:Audience"]
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtBearerEvents");

                    logger.LogInformation("Token validation successful");

                    var tenantIdClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "client_tenant_id");
                    if (tenantIdClaim != null)
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        identity?.AddClaim(new Claim("tenant_id", tenantIdClaim.Value));

                        logger.LogInformation($"Tenant ID added to claims: {tenantIdClaim.Value}");
                    }
                    else
                        logger.LogWarning("No tenant ID found in token claims");


                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("JwtBearerEvents");

                    logger.LogError($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
