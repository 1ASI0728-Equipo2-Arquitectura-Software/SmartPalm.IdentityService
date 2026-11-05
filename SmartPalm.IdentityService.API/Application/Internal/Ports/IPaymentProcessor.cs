using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Application.Abstractions;

public interface IPaymentProcessor
{
    Task<string> ProcessAsync(PaymentTransaction transaction, CancellationToken cancellationToken = default);
}
