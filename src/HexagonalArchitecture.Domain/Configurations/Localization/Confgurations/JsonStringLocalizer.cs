using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json;

namespace HexagonalArchitecture.Domain.Configurations.Localization.Confgurations;
public class JsonStringLocalizer<T> : IStringLocalizer<T>
{
    private Dictionary<string, string> _resources;

    public JsonStringLocalizer()
    {
        LoadResources();
    }

    private void LoadResources()
    {
        var culture = CultureInfo.CurrentUICulture.Name.Split('-')[0];
        var filePath = Path.Combine(AppContext.BaseDirectory, "Configurations", "Localization", "Configurations", $"{culture}.json");

        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            _resources = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
        }
        else
        {
            _resources = new Dictionary<string, string>();
        }
    }

    public LocalizedString this[string name] => new LocalizedString(name, _resources.GetValueOrDefault(name, name));

    public LocalizedString this[string name, params object[] arguments] =>
        new LocalizedString(name, string.Format(_resources.GetValueOrDefault(name, name), arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        _resources.Select(r => new LocalizedString(r.Key, r.Value));
}