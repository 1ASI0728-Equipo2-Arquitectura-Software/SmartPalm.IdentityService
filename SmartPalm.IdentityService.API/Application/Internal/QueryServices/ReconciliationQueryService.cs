using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Repositories;

namespace SmartPalm.IdentityService.Application.Services;

public class ReconciliationQueryService(
    IUserRepository users,
    ISubscriptionRepository subscriptions)
{
    public async Task<UserReconciliationSnapshotResponse?> GetUserSnapshotAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId));

        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        var subscription = await subscriptions.FindByUserIdAsync(userId, cancellationToken);
        return new UserReconciliationSnapshotResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.Status.ToString(),
            subscription?.Status.ToString());
    }
}
