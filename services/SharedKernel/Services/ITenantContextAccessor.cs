namespace SharedKernel.Services;

public interface ITenantContextAccessor
{
    Guid? TenantId { get; }
    string? TenantName { get; }
}
