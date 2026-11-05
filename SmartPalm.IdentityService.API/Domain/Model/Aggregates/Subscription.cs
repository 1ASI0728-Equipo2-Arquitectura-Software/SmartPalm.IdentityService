namespace SmartPalm.IdentityService.Domain.Subscriptions;

public class Subscription
{
    private Subscription() { }

    public Subscription(int userId, SubscriptionPlan plan, DateTime startDateUtc)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        UserId = userId;
        PlanType = plan.Type;
        PlanName = plan.Name;
        Price = plan.Price;
        BillingCycle = plan.Cycle;
        StartDate = DateTime.SpecifyKind(startDateUtc, DateTimeKind.Utc);
        EndDate = BillingCycle == BillingCycle.Yearly ? StartDate.AddYears(1) : StartDate.AddMonths(1);
        Status = SubscriptionStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public PlanType PlanType { get; private set; }
    public string PlanName { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Activate(DateTime? activatedAtUtc = null)
    {
        if (Status != SubscriptionStatus.Pending)
            throw new InvalidOperationException("Only pending subscriptions can be activated.");
        StartDate = DateTime.SpecifyKind(activatedAtUtc ?? DateTime.UtcNow, DateTimeKind.Utc);
        EndDate = BillingCycle == BillingCycle.Yearly ? StartDate.AddYears(1) : StartDate.AddMonths(1);
        Status = SubscriptionStatus.Active;
    }

    public void Cancel()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Only active subscriptions can be cancelled.");
        Status = SubscriptionStatus.Cancelled;
    }
}
