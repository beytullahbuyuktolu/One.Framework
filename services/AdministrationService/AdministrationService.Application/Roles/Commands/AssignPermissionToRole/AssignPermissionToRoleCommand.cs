using FluentValidation;
using AdministrationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Roles.Commands.AssignPermissionToRole;

public record AssignPermissionToRoleCommand : IRequest<Guid>
{
    public Guid RoleId { get; init; }
    public Guid PermissionId { get; init; }
}

public class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(v => v.RoleId).NotEmpty();
        RuleFor(v => v.PermissionId).NotEmpty();
    }
}

public class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Guid>
{
    private readonly IAdministrationDbContext _context;

    public AssignPermissionToRoleCommandHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Id == request.RoleId && r.IsActive, cancellationToken);

        if (!roleExists)
        {
            throw new Exception("Role not found or inactive");
        }

        var permissionExists = await _context.Permissions
            .AnyAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (!permissionExists)
        {
            throw new Exception("Permission not found");
        }

        var existingRolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId, cancellationToken);

        if (existingRolePermission != null)
        {
            throw new Exception("Permission is already assigned to this role");
        }

        var rolePermission = new Domain.Entities.RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = request.RoleId,
            PermissionId = request.PermissionId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync(cancellationToken);

        return rolePermission.Id;
    }
}
