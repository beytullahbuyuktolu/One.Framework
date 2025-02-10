using HexagonalArchitecture.Domain.Events;
using HexagonalArchitecture.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HexagonalArchitecture.Infrastructure.DomainEvents;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent
    {
        _logger.LogInformation("Publishing domain event: {EventName}", typeof(TEvent).Name);
        await _mediator.Publish(domainEvent, cancellationToken);
    }
}
