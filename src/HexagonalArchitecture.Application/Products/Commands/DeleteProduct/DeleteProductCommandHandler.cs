using HexagonalArchitecture.Application.Common.Models;
using HexagonalArchitecture.Domain.Interfaces;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, BaseResponse<bool>>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<BaseResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            return BaseResponse<bool>.CreateFailure($"Product with ID {request.Id} not found", new List<string> { "Product not found" });
        }

        await _productRepository.DeleteAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        return BaseResponse<bool>.CreateSuccess(true, "Product deleted successfully");
    }
}
