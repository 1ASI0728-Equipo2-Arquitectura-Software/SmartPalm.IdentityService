using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Interfaces.REST.Resources;
using SmartPalm.IdentityService.Interfaces.REST.Transform;

namespace SmartPalm.IdentityService.Api.Controllers;

[ApiController]
[Route("api/v1/Authentication")]
public class AuthenticationController(IdentityCommandService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("sign-in")]
    [ProducesResponseType<AuthenticatedUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn(SignInResource resource, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.SignInAsync(resource.ToCommand(), cancellationToken));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }
}
