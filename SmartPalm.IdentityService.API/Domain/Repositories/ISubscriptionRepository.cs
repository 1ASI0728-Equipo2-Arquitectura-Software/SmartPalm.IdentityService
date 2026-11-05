using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Domain.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Subscription?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Subscription>> ListAsync(CancellationToken cancellationToken = default);
    void Add(Subscription subscription);
}
