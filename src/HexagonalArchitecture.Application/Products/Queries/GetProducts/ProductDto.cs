using HexagonalArchitecture.Application.Common.Mappings;
using HexagonalArchitecture.Domain.Entities;

namespace HexagonalArchitecture.Application.Products.Queries.GetProducts;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
}
