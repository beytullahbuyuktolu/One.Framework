using HexagonalArchitecture.Domain.Configurations.Localization.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace HexagonalArchitecture.Domain.Configurations.Localization.Confgurations;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOneLocalization(this IServiceCollection services)
    {
        services.Configure<LocalizationOptions>(options =>
        {
            options.ResourcesPath = "Configurations/Localization/Configurations";
        });

        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.AddTransient(typeof(IStringLocalizer<>), typeof(JsonStringLocalizer<>));
        services.AddMemoryCache();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("tr")
            };

            options.DefaultRequestCulture = new RequestCulture("en");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        return services;
    }
}