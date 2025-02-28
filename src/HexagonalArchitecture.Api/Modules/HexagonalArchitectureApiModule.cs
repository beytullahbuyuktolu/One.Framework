using HexagonalArchitecture.Application;
using HexagonalArchitecture.Domain.Configurations.Localization.Confgurations;
using HexagonalArchitecture.Domain.Middlewares;
using HexagonalArchitecture.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Events;
using Serilog;
using System.Globalization;
using System.Security.Claims;
using HexagonalArchitecture.Api.Middlewares;
using HexagonalArchitecture.Domain.Configurations.KeyCloak.Interfaces;
using HexagonalArchitecture.Domain.Configurations.KeyCloak.Services;
using HexagonalArchitecture.Api.Extensions;
using HexagonalArchitecture.Domain.Permissions;
using System.Text.Json;

namespace HexagonalArchitecture.Api.Modules;

public static class HexagonalArchitectureApiModule
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        ConfigureLogging(builder);
        ConfigureCors(builder.Services);
        ConfigureSwagger(builder.Services,builder);
        ConfigureAuthentication(builder);
        ConfigureAuthorization(builder.Services);
        ConfigureMiddlewares(builder.Services);
        ConfigureApplicationServices(builder.Services, builder.Configuration);
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine("Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    private static void ConfigureSwagger(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hexagonal Architecture API", Version = "v1" });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{builder.Configuration["Keycloak:Authority"]}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect" },
                            { "profile", "User profile" },
                            { "email", "User email" }
                        }
                    }
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email" }
                }
            });
        });
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var configuration = builder.Configuration;

            options.Authority = configuration["Keycloak:Authority"];
            options.Audience = configuration["Keycloak:Audience"];
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Keycloak:Authority"],
                ValidAudience = configuration["Keycloak:Audience"],
                ValidAudiences = new[] { "broker", "account", "hexagonal-api" },
                RoleClaimType = "realm_access",
                NameClaimType = "preferred_username"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token received: {Token}", context.Token);
                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(OnePermissions.AdminPolicy, policy =>
                policy.RequireAssertion(context =>
                {
                    var logger = context.Resource as ILogger<Program>;

                    var realmAccess = context.User.Claims
                        .FirstOrDefault(c => c.Type == "realm_access")?.Value;

                    if (realmAccess != null)
                    {
                        try
                        {
                            var realmRoles = JsonSerializer.Deserialize<JsonElement>(realmAccess)
                                .GetProperty("roles")
                                .EnumerateArray()
                                .Select(r => r.GetString())
                                .ToList();

                            logger?.LogInformation("Checking admin role in realm_access. Roles: {Roles}",
                                string.Join(", ", realmRoles));

                            return realmRoles.Contains("admin");
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Error parsing realm_access roles");
                        }
                    }

                    var resourceAccess = context.User.Claims
                        .FirstOrDefault(c => c.Type == "resource_access")?.Value;

                    if (resourceAccess != null)
                    {
                        try
                        {
                            var resources = JsonSerializer.Deserialize<JsonElement>(resourceAccess);
                            if (resources.TryGetProperty("hexagonal-api", out var apiRoles))
                            {
                                var roles = apiRoles.GetProperty("roles")
                                    .EnumerateArray()
                                    .Select(r => r.GetString())
                                    .ToList();

                                logger?.LogInformation("Checking admin role in resource_access. Roles: {Roles}",
                                    string.Join(", ", roles));

                                return roles.Contains("admin");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger?.LogError(ex, "Error parsing resource_access roles");
                        }
                    }

                    logger?.LogWarning("No admin role found in either realm_access or resource_access");
                    return false;
                }));
        });
    }

    private static void ConfigureMiddlewares(IServiceCollection services)
    {
        services.AddTransient<ExceptionHandlingMiddleware>();
        services.AddScoped<LoggingMiddleware>();
    }

    private static void ConfigureApplicationServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddOneLocalization();
        services.AddHttpClient();
        services.AddScoped<IKeycloakSyncService, KeycloakSyncService>();
        services.AddHostedService<PermissionSyncHostedService>();
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(localizationOptions.Value);

        app.UseMiddleware<LoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<AuthenticationLoggingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hexagonal Architecture API V1");
                c.OAuthClientId("hexagonal-api");
                c.OAuthClientSecret("hexagonal-api-secret");
                c.OAuthScopes("openid", "profile", "email");
                c.OAuthUsePkce();
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
} 