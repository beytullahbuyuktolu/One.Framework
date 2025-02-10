namespace HexagonalArchitecture.Domain.Entities;

/// <summary>
/// Base class for entities that need full audit information (creation, modification and deletion)
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>
    where TKey : struct
{
    /// <summary>
    /// True if this entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Deleter of this entity.
    /// </summary>
    public string? DeletedBy { get; set; }
}
