using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Model.Queries;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Services;
using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Application.Services;

public class SubscriptionQueryService(
    ISubscriptionRepository subscriptions,
    IPaymentRepository payments)
{
    public IReadOnlyCollection<PlanResponse> Handle(GetSubscriptionPlansQuery query) =>
        SubscriptionPlanCatalog.GetAll()
            .Select(plan => new PlanResponse(
                plan.Type.ToString(), plan.Name, plan.Price, plan.Cycle.ToString(),
                plan.MaxHectares, plan.MaxSensors, plan.MaxPlantationHistory))
            .ToArray();

    public async Task<object?> HandleAsync(
        GetSubscriptionByUserIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptions.FindByUserIdAsync(query.UserId, cancellationToken);
        if (subscription is null) return null;
        return subscription.Status == SubscriptionStatus.Active
            ? ToResponse(subscription)
            : new InactiveSubscriptionResponse(
                subscription.Status.ToString(), subscription.PlanName, subscription.Price);
    }

    public async Task<SubscriptionResponse?> HandleAsync(
        GetSubscriptionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptions.FindByIdAsync(query.SubscriptionId, cancellationToken);
        return subscription is null ? null : ToResponse(subscription);
    }

    public async Task<IReadOnlyCollection<SubscriptionResponse>> HandleAsync(
        GetAllSubscriptionsQuery query,
        CancellationToken cancellationToken = default) =>
        (await subscriptions.ListAsync(cancellationToken)).Select(ToResponse).ToArray();

    public async Task<IReadOnlyCollection<PaymentResponse>> HandleAsync(
        GetPaymentsByUserIdQuery query,
        CancellationToken cancellationToken = default) =>
        (await payments.ListByUserIdAsync(query.UserId, cancellationToken)).Select(ToResponse).ToArray();

    private static SubscriptionResponse ToResponse(Subscription entity) => new(
        entity.PlanType.ToString(),
        entity.PlanName,
        entity.Price,
        entity.Status.ToString(),
        entity.StartDate.ToString("O"),
        entity.EndDate.ToString("O"),
        entity.BillingCycle.ToString(),
        entity.CreatedAt.ToString("O"));

    private static PaymentResponse ToResponse(PaymentTransaction entity) => new(
        entity.PlanName,
        entity.PeriodStart.ToString("O"),
        entity.PeriodEnd.ToString("O"),
        entity.Amount,
        entity.Status.ToString(),
        entity.ProcessedAt.ToString("O"));
}
