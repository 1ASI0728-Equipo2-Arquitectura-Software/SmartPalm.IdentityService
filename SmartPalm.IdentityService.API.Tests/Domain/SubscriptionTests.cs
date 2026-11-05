using SmartPalm.IdentityService.Domain.Subscriptions;
using SmartPalm.IdentityService.Domain.Services;

namespace SmartPalm.IdentityService.UnitTests;

public class SubscriptionTests
{
    [Fact]
    public void NewSubscription_IsPendingForOneBillingPeriod()
    {
        var start = new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        var subscription = new Subscription(42, SubscriptionPlanCatalog.Get(PlanType.Seed), start);

        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.Equal(start.AddMonths(1), subscription.EndDate);
        Assert.Equal(149m, subscription.Price);
    }

    [Fact]
    public void ActivateThenCancel_FollowsLifecycle()
    {
        var subscription = new Subscription(42, SubscriptionPlanCatalog.Get(PlanType.Harvest), DateTime.UtcNow);
        subscription.Activate();
        subscription.Cancel();
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public void PendingSubscription_CannotBeCancelled()
    {
        var subscription = new Subscription(42, SubscriptionPlanCatalog.Get(PlanType.Seed), DateTime.UtcNow);
        Assert.Throws<InvalidOperationException>(subscription.Cancel);
    }

    [Fact]
    public void Catalog_PreservesMonolithPlanContract()
    {
        var plans = SubscriptionPlanCatalog.GetAll();
        Assert.Collection(
            plans.OrderBy(plan => plan.Type),
            seed => Assert.Equal((PlanType.Seed, 149m), (seed.Type, seed.Price)),
            harvest => Assert.Equal((PlanType.Harvest, 349m), (harvest.Type, harvest.Price)),
            custom => Assert.Equal((PlanType.Custom, 0m), (custom.Type, custom.Price)));
    }
}
