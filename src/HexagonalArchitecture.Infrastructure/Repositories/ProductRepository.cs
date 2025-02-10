using HexagonalArchitecture.Domain.Entities;
using HexagonalArchitecture.Domain.Interfaces;
using HexagonalArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HexagonalArchitecture.Infrastructure.Repositories;

public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context, ICurrentTenantService currentTenantService) 
        : base(context, currentTenantService)
    {
    }

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetTenantFilteredQuery()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await GetTenantFilteredQuery()
            .AnyAsync(x => x.Code == code, cancellationToken);
    }
}
