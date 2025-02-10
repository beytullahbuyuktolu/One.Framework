using FluentValidation;

namespace HexagonalArchitecture.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(v => v.TenantId)
            .NotEmpty().WithMessage("TenantId is required");
    }
}
