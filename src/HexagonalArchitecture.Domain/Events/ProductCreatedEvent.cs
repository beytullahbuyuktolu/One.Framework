namespace HexagonalArchitecture.Domain.Events;

public record ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; }

    public ProductCreatedEvent(Guid productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}
