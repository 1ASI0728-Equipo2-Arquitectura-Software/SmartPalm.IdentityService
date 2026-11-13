using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Application.Services;

namespace SmartPalm.IdentityService.Api.Controllers;

[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/v1/reconciliation")]
public class ReconciliationController(ReconciliationQueryService queryService) : ControllerBase
{
    [HttpGet("users/{userId:int}/snapshot")]
    public async Task<IActionResult> GetUserSnapshot(
        int userId,
        CancellationToken cancellationToken)
    {
        var snapshot = await queryService.GetUserSnapshotAsync(userId, cancellationToken);
        return snapshot is null
            ? NotFound(new { message = "User not found." })
            : Ok(snapshot);
    }
}
