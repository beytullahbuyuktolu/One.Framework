namespace HexagonalArchitecture.Domain.Events;

public record ProductDeletedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public DateTime OccurredOn { get; }

    public ProductDeletedEvent(Guid productId)
    {
        ProductId = productId;
        OccurredOn = DateTime.UtcNow;
    }
}
