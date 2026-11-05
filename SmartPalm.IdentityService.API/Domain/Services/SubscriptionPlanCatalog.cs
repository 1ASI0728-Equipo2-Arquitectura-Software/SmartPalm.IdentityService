using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Domain.Services;

public static class SubscriptionPlanCatalog
{
    public static SubscriptionPlan Get(PlanType type) => type switch
    {
        PlanType.Seed => new(type, "Seed", 149m, BillingCycle.Monthly, 50, 20, 3),
        PlanType.Harvest => new(type, "Harvest", 349m, BillingCycle.Monthly, null, null, null),
        PlanType.Custom => new(type, "Custom", 0m, BillingCycle.Monthly, null, null, null),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static IReadOnlyCollection<SubscriptionPlan> GetAll() =>
        Enum.GetValues<PlanType>().Select(Get).ToArray();
}
