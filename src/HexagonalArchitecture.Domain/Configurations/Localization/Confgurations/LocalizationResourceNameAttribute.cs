namespace HexagonalArchitecture.Domain.Configurations.Localization;
[AttributeUsage(AttributeTargets.Class)]
public class LocalizationResourceNameAttribute : Attribute
{
    public string Name { get; }
    public LocalizationResourceNameAttribute(string name)
    {
        Name = name;
    }
}