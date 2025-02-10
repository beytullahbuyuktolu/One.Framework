using HexagonalArchitecture.Domain.Common;
using HexagonalArchitecture.Domain.Interfaces;
using HexagonalArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HexagonalArchitecture.Infrastructure.Repositories;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
    where TKey : struct
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ICurrentTenantService _currentTenantService;

    public Repository(ApplicationDbContext context, ICurrentTenantService currentTenantService)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _currentTenantService = currentTenantService;
    }

    protected IQueryable<TEntity> GetTenantFilteredQuery()
    {
        var query = _dbSet.AsQueryable();
        if (typeof(IMustHaveTenant).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((IMustHaveTenant)e).TenantId == _currentTenantService.GetTenantId());
        }
        return query;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetTenantFilteredQuery().ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await GetTenantFilteredQuery()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await GetTenantFilteredQuery().CountAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null && entity is IMustHaveTenant tenantEntity && tenantEntity.TenantId != _currentTenantService.GetTenantId())
        {
            return null;
        }
        return entity;
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is IMustHaveTenant)
        {
            var tenantIdProp = entity.GetType().GetProperty("TenantId");
            tenantIdProp?.SetValue(entity, _currentTenantService.GetTenantId());
        }
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is IMustHaveTenant tenantEntity && tenantEntity.TenantId != _currentTenantService.GetTenantId())
        {
            throw new UnauthorizedAccessException("Cannot update entity belonging to different tenant");
        }
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is IMustHaveTenant tenantEntity && tenantEntity.TenantId != _currentTenantService.GetTenantId())
        {
            throw new UnauthorizedAccessException("Cannot delete entity belonging to different tenant");
        }
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
