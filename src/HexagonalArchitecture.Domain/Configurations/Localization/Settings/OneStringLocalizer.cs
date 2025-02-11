using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace HexagonalArchitecture.Domain.Configurations.Localization.Settings;
public class OneStringLocalizer<TResource> : IStringLocalizer<TResource>
{
    private readonly string _resourceName;
    private readonly IMemoryCache _cache;

    public OneStringLocalizer(IMemoryCache cache)
    {
        var attribute = typeof(TResource).GetCustomAttribute<LocalizationResourceNameAttribute>();
        _resourceName = attribute?.Name ?? typeof(TResource).Name;
        _cache = cache;
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = GetString(name);
            var value = string.Format(format ?? name, arguments);
            return new LocalizedString(name, value);
        }
    }

    private string GetString(string key)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var cacheKey = $"{_resourceName}_{culture}";

        var resources = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);
            return LoadResources(culture);
        });

        return resources.TryGetValue(key, out var value) ? value : null;
    }

    private Dictionary<string, string> LoadResources(string culture)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory,
            "Resources",
            "One",
            $"{culture}.json");

        if (!File.Exists(filePath))
            return new Dictionary<string, string>();

        var jsonContent = File.ReadAllText(filePath);
        var resourceObject = JsonSerializer.Deserialize<LocalizationJsonResource>(jsonContent);
        return resourceObject?.Texts ?? new Dictionary<string, string>();
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var resources = LoadResources(culture);

        return resources.Select(r => new LocalizedString(r.Key, r.Value));
    }
}

public class LocalizationJsonResource
{
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
    public Dictionary<string, string> Texts { get; set; } = new();
}