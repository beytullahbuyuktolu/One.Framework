using HexagonalArchitecture.Application.Common.Models;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;
using HexagonalArchitecture.Domain.Interfaces;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, BaseResponse<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return BaseResponse<ProductDto>.CreateFailure($"Product with ID {request.Id} not found", new List<string> { "Product not found" });
        }

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Code = product.Code,
            Price = product.Price
        };

        return BaseResponse<ProductDto>.CreateSuccess(productDto);
    }
}
