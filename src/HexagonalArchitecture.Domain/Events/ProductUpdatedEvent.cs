namespace HexagonalArchitecture.Domain.Events;

public record ProductUpdatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; }

    public ProductUpdatedEvent(Guid productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}
