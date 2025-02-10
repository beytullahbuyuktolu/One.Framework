using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Globalization;

namespace SharedKernel.Localization
{
    public interface ILocalizationService
    {
        string GetString(string key);
        string GetString(string key, params object[] args);
        string GetString(string key, CultureInfo culture);
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IStringLocalizer _localizer;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _cache;

        public LocalizationService(IStringLocalizer localizer)
        {
            _localizer = localizer;
            _cache = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        }

        public string GetString(string key)
        {
            return GetString(key, CultureInfo.CurrentUICulture);
        }

        public string GetString(string key, params object[] args)
        {
            var value = GetString(key);
            return args == null || args.Length == 0 ? value : string.Format(value, args);
        }

        public string GetString(string key, CultureInfo culture)
        {
            var cultureName = culture.Name;
            
            if (!_cache.TryGetValue(cultureName, out var cultureCache))
            {
                cultureCache = new ConcurrentDictionary<string, string>();
                _cache[cultureName] = cultureCache;
            }

            if (!cultureCache.TryGetValue(key, out var value))
            {
                value = _localizer[key, culture].Value;
                cultureCache[key] = value;
            }

            return value;
        }
    }
}
