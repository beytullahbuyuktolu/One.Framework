using HexagonalArchitecture.Application.Common.Models;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<PagedResponse<ProductDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
