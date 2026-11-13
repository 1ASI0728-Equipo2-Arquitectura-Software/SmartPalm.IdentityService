using Microsoft.EntityFrameworkCore;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

internal class SubscriptionRepository(IdentityDbContext context) : ISubscriptionRepository
{
    public Task<Subscription?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Subscriptions.SingleOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);

    public Task<Subscription?> FindByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        context.Subscriptions
            .OrderByDescending(subscription => subscription.CreatedAt)
            .FirstOrDefaultAsync(subscription => subscription.UserId == userId, cancellationToken);

    public async Task<IReadOnlyCollection<Subscription>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await context.Subscriptions.AsNoTracking().OrderBy(subscription => subscription.Id)
            .ToArrayAsync(cancellationToken);

    public void Add(Subscription subscription) => context.Subscriptions.Add(subscription);
    public void RemoveByUserId(int userId) => context.Subscriptions.RemoveRange(
        context.Subscriptions.Where(subscription => subscription.UserId == userId));
}
