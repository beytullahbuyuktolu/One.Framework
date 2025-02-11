namespace HexagonalArchitecture.Domain.Configurations.Localization;
public class LocalizationResource
{
    public Type ResourceType { get; }
    public List<Type> BaseResourceTypes { get; }

    public LocalizationResource(Type resourceType)
    {
        ResourceType = resourceType;
        BaseResourceTypes = new List<Type>();
    }
    public LocalizationResource AddBaseTypes(params Type[] types)
    {
        BaseResourceTypes.AddRange(types);
        return this;
    }
}