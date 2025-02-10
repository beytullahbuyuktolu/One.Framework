namespace HexagonalArchitecture.Domain.Interfaces;

public interface IMustHaveTenant
{
    Guid TenantId { get; }
}
