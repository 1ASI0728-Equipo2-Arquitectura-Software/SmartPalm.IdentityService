using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Model.Queries;

namespace SmartPalm.IdentityService.Api.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/v1/users")]
public class UsersController(IdentityQueryService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken) =>
        Ok(await service.HandleAsync(new GetAllUsersQuery(), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await service.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
    }
}
