namespace HexagonalArchitecture.Domain.Configurations.Localization;
public class LocalizationResourceDictionary : Dictionary<Type, LocalizationResource>
{
    public LocalizationResource Get<TResource>()
    {
        return this[typeof(TResource)];
    }
    public LocalizationResource Add<TResource>(LocalizationResource resource)
    {
        this[typeof(TResource)] = resource;
        return resource;
    }
}