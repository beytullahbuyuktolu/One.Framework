using System;
using MediatR;

namespace HexagonalArchitecture.Domain.Events
{
    public abstract class DomainEvent : INotification
    {
        public DateTime OccurredOn { get; }
        public Guid EventId { get; }

        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}
