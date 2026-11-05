namespace SmartPalm.IdentityService.Domain.Subscriptions;

public record SubscriptionPlan(
    PlanType Type,
    string Name,
    decimal Price,
    BillingCycle Cycle,
    int? MaxHectares,
    int? MaxSensors,
    int? MaxPlantationHistory);
