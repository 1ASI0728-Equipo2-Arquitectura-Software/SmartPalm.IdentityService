using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Model.Commands;
using SmartPalm.IdentityService.Domain.Events;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Services;
using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Application.Services;

public class SubscriptionCommandService(
    IUserRepository users,
    ISubscriptionRepository subscriptions,
    IPaymentRepository payments,
    IPaymentProcessor paymentProcessor,
    IIntegrationEventWriter eventWriter,
    IUnitOfWork unitOfWork)
{
    public async Task<SubscriptionResponse> CreateAsync(
        CreateSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await users.FindByIdAsync(command.UserId, cancellationToken) is null)
            throw new KeyNotFoundException("User not found.");
        if (!Enum.TryParse<PlanType>(command.PlanType, true, out var planType))
            throw new ArgumentException($"Invalid plan type '{command.PlanType}'.");

        var existing = await subscriptions.FindByUserIdAsync(command.UserId, cancellationToken);
        if (existing is not null && existing.Status is SubscriptionStatus.Active or SubscriptionStatus.Pending)
            throw new InvalidOperationException("The user already has an active or pending subscription.");

        var subscription = new Subscription(command.UserId, SubscriptionPlanCatalog.Get(planType), DateTime.UtcNow);
        return await unitOfWork.ExecuteInTransactionAsync(async innerToken =>
        {
            subscriptions.Add(subscription);
            await unitOfWork.SaveChangesAsync(innerToken);
            eventWriter.Enqueue(
                "smartpalm.identity.subscription-created.v1",
                new SubscriptionCreatedIntegrationEvent(
                    subscription.Id,
                    subscription.UserId,
                    subscription.PlanType.ToString(),
                    subscription.Status.ToString(),
                    DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(innerToken);
            return ToResponse(subscription);
        }, cancellationToken);
    }

    public async Task<SubscriptionResponse> CancelAsync(
        CancelSubscriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptions.FindByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");
        subscription.Cancel();
        eventWriter.Enqueue(
            "smartpalm.identity.subscription-cancelled.v1",
            new SubscriptionCancelledIntegrationEvent(
                subscription.Id,
                subscription.UserId,
                DateTime.UtcNow));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(subscription);
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
        var subscription = await subscriptions.FindByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Subscription not found.");
        if (subscription.Status != SubscriptionStatus.Pending)
            throw new InvalidOperationException("Only pending subscriptions can be paid.");

        var periodStart = DateTime.UtcNow;
        var periodEnd = subscription.BillingCycle == BillingCycle.Yearly
            ? periodStart.AddYears(1)
            : periodStart.AddMonths(1);

        var payment = new PaymentTransaction(
            command.UserId,
            subscription.PlanName,
            periodStart,
            periodEnd,
            command.Amount);

        return await unitOfWork.ExecuteInTransactionAsync(async innerToken =>
        {
            payments.Add(payment);
            var transactionId = await paymentProcessor.ProcessAsync(payment, innerToken);
            payment.Complete(transactionId);
            subscription.Activate(periodStart);
            await unitOfWork.SaveChangesAsync(innerToken);
            eventWriter.Enqueue(
                "smartpalm.identity.subscription-activated.v1",
                new SubscriptionActivatedIntegrationEvent(
                    subscription.Id,
                    subscription.UserId,
                    subscription.PlanType.ToString(),
                    subscription.StartDate,
                    subscription.EndDate,
                    DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(innerToken);
            return ToResponse(payment);
        }, cancellationToken);
    }

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
