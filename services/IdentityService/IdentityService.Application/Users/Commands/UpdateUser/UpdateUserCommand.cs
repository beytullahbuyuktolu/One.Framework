using FluentValidation;
using IdentityService.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Users.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<bool>
{
    public Guid UserId { get; init; }
    public string? Email { get; init; }
    public string? NewPassword { get; init; }
    public bool? IsActive { get; init; }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        
        When(v => v.Email != null, () =>
        {
            RuleFor(v => v.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(100);
        });

        When(v => v.NewPassword != null, () =>
        {
            RuleFor(v => v.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(100)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        });
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IIdentityDbContext _context;

    public UpdateUserCommandHandler(IIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        if (request.Email != null)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId, cancellationToken);

            if (emailExists)
            {
                throw new Exception("Email already exists");
            }

            user.Email = request.Email;
        }

        if (request.NewPassword != null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
