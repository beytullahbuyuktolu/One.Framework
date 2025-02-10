namespace StockService.Domain.Entities;

public class Stock
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? UpdatedDate { get; private set; }
    public string TenantId { get; private set; }

    private Stock() { }

    public Stock(string name, string code, int quantity, decimal price, string tenantId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        Quantity = quantity;
        Price = price;
        CreatedDate = DateTime.UtcNow;
        TenantId = tenantId;
    }

    public void UpdateStock(int quantity, decimal price)
    {
        Quantity = quantity;
        Price = price;
        UpdatedDate = DateTime.UtcNow;
    }
}
