using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Model.Commands;
using SmartPalm.IdentityService.Domain.Model.Queries;

namespace SmartPalm.IdentityService.Api.Controllers;

[ApiController]
[Route("api/v1/subscriptions")]
public class SubscriptionsController(
    SubscriptionCommandService commandService,
    SubscriptionQueryService queryService) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("plans")]
    public IActionResult ListPlans() => Ok(queryService.Handle(new GetSubscriptionPlansQuery()));

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetSubscription(CancellationToken cancellationToken)
    {
        var subscription = await queryService.HandleAsync(
            new GetSubscriptionByUserIdQuery(CurrentUserId()), cancellationToken);
        return subscription is null
            ? NotFound(new { message = "Subscription not found." })
            : Ok(subscription);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await commandService.CancelAsync(
                new CancelSubscriptionCommand(CurrentUserId()), cancellationToken);
            return Ok(new { message = "Subscription cancelled.", status = subscription.status });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize]
    [HttpGet("payments")]
    public async Task<IActionResult> ListPayments(CancellationToken cancellationToken) =>
        Ok(await queryService.HandleAsync(
            new GetPaymentsByUserIdQuery(CurrentUserId()), cancellationToken));

    private int CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue("sub");
        return int.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Token does not contain a valid user id.");
    }
}
