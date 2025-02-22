using HexagonalArchitecture.Application.Common.Models;
using HexagonalArchitecture.Application.Products.Queries.GetProducts;
using HexagonalArchitecture.Domain.Entities;
using HexagonalArchitecture.Domain.Interfaces;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly ICurrentTenantService _currentTenantService;

    public CreateProductCommandHandler(IProductRepository productRepository, ICurrentTenantService currentTenantService)
    {
        _productRepository = productRepository;
        _currentTenantService = currentTenantService;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Code, request.Price, _currentTenantService.GetTenantId());
        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
