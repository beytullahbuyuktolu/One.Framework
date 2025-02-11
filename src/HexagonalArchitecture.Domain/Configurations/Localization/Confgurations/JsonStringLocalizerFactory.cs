using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace HexagonalArchitecture.Domain.Configurations.Localization.Confgurations;
public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer<object>();
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer<object>();
    }
}