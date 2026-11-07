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
    IdentityQueryService queryService,
    UserDeletionCommandService deletionCommandService,
    UserDeletionQueryService deletionQueryService) : ControllerBase
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

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = int.TryParse(User.FindFirst("sub")?.Value, out var parsed) ? parsed : 0;
            var operation = await deletionCommandService.DeleteAsync(userId, requesterId, cancellationToken);
            return Accepted(
                $"/api/v1/admin/users/deletion-operations/{operation.operationId}",
                operation);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpGet("deletion-operations/{operationId:guid}")]
    public async Task<IActionResult> GetDeletionOperation(Guid operationId, CancellationToken cancellationToken)
    {
        var operation = await deletionQueryService.GetAsync(operationId, cancellationToken);
        return operation is null
            ? NotFound(new { message = "Deletion operation not found." })
            : Ok(operation);
    }
}
