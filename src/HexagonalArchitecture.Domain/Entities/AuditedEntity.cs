using HexagonalArchitecture.Domain.Common;

namespace HexagonalArchitecture.Domain.Entities;

/// <summary>
/// Base class for entities that need basic audit information
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
public abstract class AuditedEntity<TKey> : Entity<TKey>
    where TKey : struct
{
    /// <summary>
    /// Creation time of this entity.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Creator of this entity.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Last modification time of this entity.
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// Last modifier of this entity.
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
