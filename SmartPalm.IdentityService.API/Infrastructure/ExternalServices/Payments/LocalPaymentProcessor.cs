using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Infrastructure.Payments;

internal class LocalPaymentProcessor : IPaymentProcessor
{
    public Task<string> ProcessAsync(
        PaymentTransaction transaction,
        CancellationToken cancellationToken = default) =>
        Task.FromResult($"local_{Guid.NewGuid():N}");
}
