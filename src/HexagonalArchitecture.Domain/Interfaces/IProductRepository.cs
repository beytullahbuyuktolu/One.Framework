using HexagonalArchitecture.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HexagonalArchitecture.Domain.Interfaces;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);
}
