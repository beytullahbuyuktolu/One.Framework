using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace HexagonalArchitecture.Domain.Configurations.Localization.Confgurations;
public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;

    public JsonStringLocalizerFactory(IOptions<LocalizationOptions> options)
    {
        _resourcesPath = options.Value.ResourcesPath;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer();
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer();
    }
}