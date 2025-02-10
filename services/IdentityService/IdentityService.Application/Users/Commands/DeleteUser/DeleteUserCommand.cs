using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel.Infrastructure.Repository;
using SharedKernel.Domain;
using SharedKernel.Messaging;

namespace IdentityService.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;
    private readonly IMessageBus _messageBus;

    public DeleteUserCommandHandler(
        IRepository<User, Guid> userRepository,
        ILogger<DeleteUserCommandHandler> logger,
        IMessageBus messageBus)
    {
        _userRepository = userRepository;
        _logger = logger;
        _messageBus = messageBus;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", request.UserId);
            throw new InvalidOperationException($"User not found: {request.UserId}");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        await _messageBus.PublishAsync(new UserDeletedEvent(user.Id), "user-deleted", cancellationToken);

        _logger.LogInformation("User deleted: {UserId}", request.UserId);
    }
}

public record UserDeletedEvent(Guid UserId);
