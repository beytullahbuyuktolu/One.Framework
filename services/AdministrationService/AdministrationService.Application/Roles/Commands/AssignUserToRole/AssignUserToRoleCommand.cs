using FluentValidation;
using AdministrationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Roles.Commands.AssignUserToRole;

public record AssignUserToRoleCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
}

public class AssignUserToRoleCommandValidator : AbstractValidator<AssignUserToRoleCommand>
{
    public AssignUserToRoleCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.RoleId).NotEmpty();
    }
}

public class AssignUserToRoleCommandHandler : IRequestHandler<AssignUserToRoleCommand, Guid>
{
    private readonly IAdministrationDbContext _context;

    public AssignUserToRoleCommandHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignUserToRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await _context.Roles
            .AnyAsync(r => r.Id == request.RoleId && r.IsActive, cancellationToken);

        if (!roleExists)
        {
            throw new Exception("Role not found or inactive");
        }

        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (existingUserRole != null)
        {
            throw new Exception("User is already assigned to this role");
        }

        var userRole = new Domain.Entities.UserRole
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return userRole.Id;
    }
}
