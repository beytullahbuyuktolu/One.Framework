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
                        AuthorizationUrl = new Uri($"{builder.Configuration["Keycloak:Authority"]}/realms/hexagonal-architecture/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{builder.Configuration["Keycloak:Authority"]}/realms/hexagonal-architecture/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID bağlantısı" },
                            { "profile", "Profil bilgileri" },
                            { "email", "E-posta bilgisi" }
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
            var keycloakAuthority = configuration["Keycloak:Authority"];

            options.Authority = keycloakAuthority;
            options.RequireHttpsMetadata = false;
            options.MetadataAddress = $"{keycloakAuthority}/realms/hexagonal-architecture/.well-known/openid-configuration";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = $"{keycloakAuthority}/realms/hexagonal-architecture",
                RoleClaimType = "realm_access",
                NameClaimType = "preferred_username",
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token başarıyla doğrulandı: {User}", context.Principal?.Identity?.Name);
                    
                    var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                    if (token != null)
                    {
                        logger.LogInformation("Token Azp: {Azp}", token.Payload.Azp ?? "Azp değeri bulunamadı");
                        logger.LogInformation("Token Claims: {Claims}", 
                            string.Join(", ", token.Claims.Select(c => $"{c.Type}: {c.Value}")));
                    }
                    
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(context.Exception, "Kimlik doğrulama başarısız: {Message}", context.Exception.Message);
                    logger.LogError("İstisna tipi: {ExceptionType}, Ayrıntılar: {ExceptionDetails}", 
                        context.Exception.GetType().Name, context.Exception.ToString());
                    if (context.Exception is SecurityTokenInvalidAudienceException)
                    {
                        logger.LogError("Token audience hatası: Token içindeki audience değerleri geçerli değil");
                    }
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token alındı: {Token}", context.Token?.Substring(0, Math.Min(50, context.Token?.Length ?? 0)) + "...");
                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void ConfigureAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireAssertion(context =>
                {
                    var realmAccess = context.User.FindFirst("realm_access")?.Value;
                    if (string.IsNullOrEmpty(realmAccess)) return false;

                    try
                    {
                        var realmRoles = JsonSerializer.Deserialize<JsonElement>(realmAccess)
                            .GetProperty("roles")
                            .EnumerateArray()
                            .Select(r => r.GetString())
                            .ToList();

                        return realmRoles.Contains("admin");
                    }
                    catch
                    {
                        return false;
                    }
                }));

            options.AddPolicy("Permission_One.Products.Read", policy =>
                policy.RequireAssertion(context =>
                {
                    var realmAccess = context.User.FindFirst("realm_access")?.Value;
                    if (string.IsNullOrEmpty(realmAccess)) return false;

                    try
                    {
                        var realmRoles = JsonSerializer.Deserialize<JsonElement>(realmAccess)
                            .GetProperty("roles")
                            .EnumerateArray()
                            .Select(r => r.GetString())
                            .ToList();

                        return realmRoles.Contains("admin") || realmRoles.Contains("user");
                    }
                    catch
                    {
                        return false;
                    }
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