namespace SharedKernel.MultiTenancy;

public interface ITenantInfo
{
    string Id { get; }
    string Name { get; }
    string ConnectionString { get; }
}
