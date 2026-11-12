namespace SmartPalm.IdentityService.Domain.Events;

public record SubscriptionActivatedIntegrationEvent(
    int SubscriptionId,
    int UserId,
    string PlanType,
    DateTime StartDate,
    DateTime EndDate,
    DateTime OccurredAtUtc,
    bool IsActive = true);
