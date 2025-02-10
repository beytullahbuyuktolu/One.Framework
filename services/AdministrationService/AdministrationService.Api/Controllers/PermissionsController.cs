using AdministrationService.Application.Permissions.Commands.AssignPermission;
using AdministrationService.Application.Permissions.Commands.RevokePermission;
using AdministrationService.Application.Permissions.Queries.GetUserPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Authentication;
using SharedKernel.Events;
using SharedKernel.Messaging;

namespace AdministrationService.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;
    private readonly IMessageBus _messageBus;

    public PermissionsController(
        IMediator mediator,
        ILogger<PermissionsController> logger,
        IMessageBus messageBus)
    {
        _mediator = mediator;
        _logger = logger;
        _messageBus = messageBus;
    }

    [HttpPost]
    [RequireRole("Admin")]
    [RequirePermission("Permissions.Assign")]
    public async Task<ActionResult<Guid>> AssignPermission(AssignPermissionCommand command)
    {
        try
        {
            var permissionId = await _mediator.Send(command);

            // Publish integration event
            var @event = new PermissionAssignedEvent
            {
                PermissionId = permissionId,
                UserId = command.UserId,
                PermissionKey = command.PermissionKey,
                AssignedAt = DateTime.UtcNow
            };
            
            await _messageBus.PublishAsync(@event, "permissions.assigned");

            return Ok(permissionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permission");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{userId}/permissions/{permissionKey}")]
    [RequireRole("Admin")]
    [RequirePermission("Permissions.Revoke")]
    public async Task<ActionResult> RevokePermission(Guid userId, string permissionKey)
    {
        try
        {
            var command = new RevokePermissionCommand
            {
                UserId = userId,
                PermissionKey = permissionKey
            };

            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking permission");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    [RequireRole("Admin")]
    [RequirePermission("Permissions.View")]
    public async Task<ActionResult<List<UserPermissionDto>>> GetUserPermissions(Guid userId)
    {
        try
        {
            var query = new GetUserPermissionsQuery { UserId = userId };
            var permissions = await _mediator.Send(query);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions");
            return BadRequest(new { error = ex.Message });
        }
    }
}
