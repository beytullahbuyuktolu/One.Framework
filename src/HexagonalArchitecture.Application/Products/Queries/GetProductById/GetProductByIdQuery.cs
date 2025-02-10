using HexagonalArchitecture.Application.Common.Models;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<BaseResponse<ProductDto>>
{
    public Guid Id { get; set; }
}
