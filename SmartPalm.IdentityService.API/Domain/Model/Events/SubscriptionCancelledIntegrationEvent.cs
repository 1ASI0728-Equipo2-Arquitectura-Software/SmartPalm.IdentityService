namespace SmartPalm.IdentityService.Domain.Events;

public record SubscriptionCancelledIntegrationEvent(
    int SubscriptionId,
    int UserId,
    DateTime OccurredAtUtc);
