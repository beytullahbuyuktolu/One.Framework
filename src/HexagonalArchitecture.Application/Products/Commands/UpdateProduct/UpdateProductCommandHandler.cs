using HexagonalArchitecture.Application.Common.Models;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;
using HexagonalArchitecture.Domain.Interfaces;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, BaseResponse<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return BaseResponse<ProductDto>.CreateFailure($"Product with ID {request.Id} not found", new List<string> { "Product not found" });
        }

        product.Update(request.Name, request.Code, request.Price);

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Code = product.Code,
            Price = product.Price
        };

        return BaseResponse<ProductDto>.CreateSuccess(productDto, "Product updated successfully");
    }
}
