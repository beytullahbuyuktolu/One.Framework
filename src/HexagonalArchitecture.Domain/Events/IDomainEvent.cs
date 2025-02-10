using MediatR;

namespace HexagonalArchitecture.Domain.Events;

/// <summary>
/// Interface for domain events
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The time when this domain event occurred
    /// </summary>
    DateTime OccurredOn { get; }
}
