using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Domain.Repositories;

public interface IPaymentRepository
{
    Task<IReadOnlyCollection<PaymentTransaction>> ListByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);
    void Add(PaymentTransaction payment);
    void RemoveByUserId(int userId);
}
