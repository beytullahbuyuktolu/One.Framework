namespace SharedKernel.Events;

public record PermissionAssignedEvent
{
    public Guid PermissionId { get; init; }
    public Guid UserId { get; init; }
    public string PermissionKey { get; init; } = null!;
    public DateTime AssignedAt { get; init; }
}
