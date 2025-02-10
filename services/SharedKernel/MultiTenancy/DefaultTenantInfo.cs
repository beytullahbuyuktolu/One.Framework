namespace SharedKernel.MultiTenancy;

public class DefaultTenantInfo : ITenantInfo
{
    public string Id { get; set; } = "default";
    public string Name { get; set; } = "Default";
    public string ConnectionString { get; set; } = string.Empty;
}
