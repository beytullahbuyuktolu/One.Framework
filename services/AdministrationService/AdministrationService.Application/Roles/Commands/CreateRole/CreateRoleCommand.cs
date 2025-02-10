using FluentValidation;
using AdministrationService.Application.Common.Interfaces;
using MediatR;

namespace AdministrationService.Application.Roles.Commands.CreateRole;

public record CreateRoleCommand : IRequest<Guid>
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IAdministrationDbContext _context;

    public CreateRoleCommandHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new Domain.Entities.Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return role.Id;
    }
}
