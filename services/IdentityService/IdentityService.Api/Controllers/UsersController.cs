using IdentityService.Application.Users.Commands.AuthenticateUser;
using IdentityService.Application.Users.Commands.DeleteUser;
using IdentityService.Application.Users.Commands.RegisterUser;
using IdentityService.Application.Users.Commands.UpdateUser;
using IdentityService.Application.Users.Queries.GetUser;
using IdentityService.Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Authentication;
using SharedKernel.Events;
using SharedKernel.Messaging;

namespace IdentityService.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;
    private readonly IMessageBus _messageBus;

    public UsersController(
        IMediator mediator,
        ILogger<UsersController> logger,
        IMessageBus messageBus)
    {
        _mediator = mediator;
        _logger = logger;
        _messageBus = messageBus;
    }

    [HttpPost]
    [RequireRole("Admin")]
    public async Task<ActionResult<Guid>> Create(RegisterUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);

            // Publish integration event
            var @event = new UserRegisteredEvent
            {
                UserId = userId,
                Username = command.Username,
                Email = command.Email,
                RegisteredAt = DateTime.UtcNow
            };
            
            await _messageBus.PublishAsync(@event, "users.registered");

            return Ok(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticateUserResult>> Authenticate(AuthenticateUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [RequireRole("Admin")]
    [RequirePermission("Users.View")]
    public async Task<ActionResult<UserDto>> Get(Guid id)
    {
        try
        {
            var query = new GetUserQuery { UserId = id };
            var user = await _mediator.Send(query);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [RequireRole("Admin")]
    [RequirePermission("Users.List")]
    public async Task<ActionResult<List<UserListDto>>> GetAll([FromQuery] GetUsersQuery query)
    {
        try
        {
            var users = await _mediator.Send(query);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [RequireRole("Admin")]
    [RequirePermission("Users.Edit")]
    public async Task<ActionResult> Update(Guid id, UpdateUserCommand command)
    {
        if (id != command.UserId)
        {
            return BadRequest(new { error = "User ID mismatch" });
        }

        try
        {
            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [RequireRole("Admin")]
    [RequirePermission("Users.Delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            return BadRequest(new { error = ex.Message });
        }
    }
}
