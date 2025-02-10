using FluentValidation;
using AdministrationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Permissions.Commands.AssignPermission;

public record AssignPermissionCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string PermissionKey { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public class AssignPermissionCommandValidator : AbstractValidator<AssignPermissionCommand>
{
    public AssignPermissionCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.PermissionKey)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(200);
    }
}

public class AssignPermissionCommandHandler : IRequestHandler<AssignPermissionCommand, Guid>
{
    private readonly IAdministrationDbContext _context;

    public AssignPermissionCommandHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
    {
        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.PermissionKey == request.PermissionKey, cancellationToken);

        if (existingPermission != null)
        {
            throw new Exception("Permission already assigned to user");
        }

        var permission = new Domain.Entities.Permission
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PermissionKey = request.PermissionKey,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return permission.Id;
    }
}
