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
[Route("api/v1/admin/subscriptions")]
public class AdminSubscriptionsController(
    SubscriptionCommandService commandService,
    SubscriptionQueryService queryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSubscription(
        CreateSubscriptionResource resource,
        CancellationToken cancellationToken)
    {
        try
        {
            return StatusCode(
                StatusCodes.Status201Created,
                await commandService.CreateAsync(resource.ToCommand(), cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ListSubscriptions(CancellationToken cancellationToken) =>
        Ok(await queryService.HandleAsync(new GetAllSubscriptionsQuery(), cancellationToken));

    [HttpGet("{subscriptionId:int}")]
    public async Task<IActionResult> GetSubscriptionById(int subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await queryService.HandleAsync(
            new GetSubscriptionByIdQuery(subscriptionId), cancellationToken);
        return subscription is null
            ? NotFound(new { message = "Subscription not found." })
            : Ok(subscription);
    }

    [HttpGet("users/{userId:int}")]
    public async Task<IActionResult> GetSubscriptionByUserId(
        int userId,
        CancellationToken cancellationToken)
    {
        var subscription = await queryService.HandleAdminAsync(
            new GetSubscriptionByUserIdQuery(userId), cancellationToken);
        return subscription is null
            ? NotFound(new { message = "Subscription not found." })
            : Ok(subscription);
    }

    [HttpPost("users/{userId:int}/payments")]
    public async Task<IActionResult> ProcessPayment(
        int userId,
        ProcessPaymentResource resource,
        CancellationToken cancellationToken)
    {
        try
        {
            return StatusCode(
                StatusCodes.Status201Created,
                await commandService.ProcessPaymentAsync(resource.ToCommand(userId), cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
