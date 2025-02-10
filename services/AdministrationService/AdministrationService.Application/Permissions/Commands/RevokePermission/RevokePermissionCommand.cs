using FluentValidation;
using AdministrationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Permissions.Commands.RevokePermission;

public record RevokePermissionCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
    public string PermissionKey { get; init; } = null!;
}

public class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.PermissionKey).NotEmpty();
    }
}

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, bool>
{
    private readonly IAdministrationDbContext _context;

    public RevokePermissionCommandHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.PermissionKey == request.PermissionKey, cancellationToken);

        if (permission == null)
        {
            return false;
        }

        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
