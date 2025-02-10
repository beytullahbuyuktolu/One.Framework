using FluentValidation;
using IdentityService.Application.Common.Interfaces;
using MediatR;

namespace IdentityService.Application.Tenants.Queries.GetTenants;

public record GetTenantsQuery : IRequest<List<TenantDto>>
{
    public bool IncludeInactive { get; init; }
}

public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
}

public class GetTenantsQueryHandler : IRequestHandler<GetTenantsQuery, List<TenantDto>>
{
    private readonly IIdentityDbContext _context;

    public GetTenantsQueryHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<List<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Tenants.AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new TenantDto
            {
                Id = t.Id,
                Name = t.Name,
                DisplayName = t.DisplayName,
                IsActive = t.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public bool IsActive { get; set; }
}
