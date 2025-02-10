using HexagonalArchitecture.Domain.Common;
using HexagonalArchitecture.Domain.Interfaces;

namespace HexagonalArchitecture.Domain.Entities;

public class Product : Entity<Guid>, IMustHaveTenant
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public Guid TenantId { get; private set; }

    public Product(string name, string code, decimal price, Guid tenantId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        Price = price;
        TenantId = tenantId;
    }

    private Product()
    {
        Id = Guid.NewGuid();
    }

    public void Update(string name, string code, decimal price)
    {
        Name = name;
        Code = code;
        Price = price;
    }
}
