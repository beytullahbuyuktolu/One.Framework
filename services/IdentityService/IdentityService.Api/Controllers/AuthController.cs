using IdentityService.Application.Authentication.Commands.AuthenticateUser;
using IdentityService.Application.Authentication.Commands.ForgotPassword;
using IdentityService.Application.Authentication.Commands.RefreshToken;
using IdentityService.Application.Authentication.Commands.RegisterUser;
using IdentityService.Application.Authentication.Commands.ResetPassword;
using IdentityService.Application.Authentication.Commands.VerifyEmail;
using IdentityService.Application.Tenants.Queries.GetTenants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ISender mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("tenants")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TenantDto>>> GetTenants([FromQuery] GetTenantsQuery query)
    {
        try
        {
            var tenants = await _mediator.Send(query);
            return Ok(tenants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenants");
            return BadRequest(new { error = "Failed to retrieve tenants" });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<Guid>> Register(RegisterUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return Ok(userId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return BadRequest(new { error = "Registration failed" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResult>> Login(AuthenticateUserCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return BadRequest(new { error = "Login failed" });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken(RefreshTokenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(new { error = "Token refresh failed" });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "If your email is registered, you will receive a password reset link" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return BadRequest(new { error = "Failed to process request" });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(ResetPasswordCommand command)
    {
        try
        {
            var success = await _mediator.Send(command);
            if (!success)
            {
                return BadRequest(new { error = "Invalid or expired token" });
            }
            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return BadRequest(new { error = "Failed to reset password" });
        }
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult> VerifyEmail(VerifyEmailCommand command)
    {
        try
        {
            var success = await _mediator.Send(command);
            if (!success)
            {
                return BadRequest(new { error = "Invalid or expired token" });
            }
            return Ok(new { message = "Email verified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return BadRequest(new { error = "Failed to verify email" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Clear refresh token
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user != null)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(new { error = "Logout failed" });
        }
    }
}
