using HexagonalArchitecture.Application.Common.CQRS;
using HexagonalArchitecture.Application.Common.Models;
using MediatR;

namespace HexagonalArchitecture.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
}
