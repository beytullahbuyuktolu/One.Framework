namespace HexagonalArchitecture.Domain.Entities;

/// <summary>
/// Interface for entities that support soft-delete
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// True if this entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion time of this entity.
    /// </summary>
    DateTime? DeletionTime { get; set; }

    /// <summary>
    /// Deleter of this entity.
    /// </summary>
    string DeleterId { get; set; }
}
