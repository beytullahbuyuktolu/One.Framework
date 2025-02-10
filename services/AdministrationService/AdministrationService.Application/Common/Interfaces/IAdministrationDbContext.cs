using AdministrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdministrationService.Application.Common.Interfaces;

public interface IAdministrationDbContext
{
    DbSet<Permission> Permissions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
