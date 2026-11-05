namespace SmartPalm.IdentityService.Domain.Events;

public record SubscriptionCreatedIntegrationEvent(
    int SubscriptionId,
    int UserId,
    string PlanType,
    string Status,
    DateTime OccurredAtUtc);
