using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Model.Queries;
using SmartPalm.IdentityService.Interfaces.REST.Resources;
using SmartPalm.IdentityService.Interfaces.REST.Transform;

namespace SmartPalm.IdentityService.Api.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/v1/admin/users")]
public class AdminUsersController(
    IdentityCommandService commandService,
    IdentityQueryService queryService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(CreateUserResource resource, CancellationToken cancellationToken)
    {
        try
        {
            var user = await commandService.CreateAsync(resource.ToCommand(), cancellationToken);
            return CreatedAtAction(nameof(GetUserById), new { userId = user.id }, user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListUsers(CancellationToken cancellationToken) =>
        Ok(await queryService.HandleAsync(new GetAllUsersQuery(), cancellationToken));

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUserById(int userId, CancellationToken cancellationToken)
    {
        var user = await queryService.HandleAsync(new GetUserByIdQuery(userId), cancellationToken);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(user);
    }
}
