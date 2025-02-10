using HexagonalArchitecture.Application.Common.CQRS;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;

namespace HexagonalArchitecture.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommand : ICommand<ProductDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Price { get; set; }
}
