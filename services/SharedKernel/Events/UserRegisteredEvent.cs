namespace SharedKernel.Events;

public record UserRegisteredEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public DateTime RegisteredAt { get; init; }
}
