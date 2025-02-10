using AdministrationService.Application.Roles.Commands.AssignPermissionToRole;
using AdministrationService.Application.Roles.Commands.AssignUserToRole;
using AdministrationService.Application.Roles.Commands.CreateRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Authentication;

namespace AdministrationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ISender _mediator;

    public RolesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequirePermission("Roles.Create")]
    public async Task<ActionResult<Guid>> Create(CreateRoleCommand command)
    {
        var roleId = await _mediator.Send(command);
        return Ok(roleId);
    }

    [HttpPost("{roleId}/users")]
    [RequirePermission("Roles.AssignUser")]
    public async Task<ActionResult<Guid>> AssignUser(Guid roleId, [FromBody] AssignUserToRoleCommand command)
    {
        if (roleId != command.RoleId)
        {
            return BadRequest();
        }

        var userRoleId = await _mediator.Send(command);
        return Ok(userRoleId);
    }

    [HttpPost("{roleId}/permissions")]
    [RequirePermission("Roles.AssignPermission")]
    public async Task<ActionResult<Guid>> AssignPermission(Guid roleId, [FromBody] AssignPermissionToRoleCommand command)
    {
        if (roleId != command.RoleId)
        {
            return BadRequest();
        }

        var rolePermissionId = await _mediator.Send(command);
        return Ok(rolePermissionId);
    }
}
