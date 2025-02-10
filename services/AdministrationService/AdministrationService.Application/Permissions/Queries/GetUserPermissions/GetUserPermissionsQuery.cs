using AdministrationService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Permissions.Queries.GetUserPermissions;

public record GetUserPermissionsQuery : IRequest<List<UserPermissionDto>>
{
    public Guid UserId { get; init; }
}

public record UserPermissionDto
{
    public Guid Id { get; init; }
    public string PermissionKey { get; init; } = null!;
    public string Description { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, List<UserPermissionDto>>
{
    private readonly IAdministrationDbContext _context;

    public GetUserPermissionsQueryHandler(IAdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserPermissionDto>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Permissions
            .Where(p => p.UserId == request.UserId)
            .Select(p => new UserPermissionDto
            {
                Id = p.Id,
                PermissionKey = p.PermissionKey,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
