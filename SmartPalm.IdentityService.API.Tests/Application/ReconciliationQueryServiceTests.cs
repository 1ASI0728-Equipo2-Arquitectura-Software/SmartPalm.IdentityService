using System.Text.Json;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Services;
using SmartPalm.IdentityService.Domain.Subscriptions;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.API.Tests.Application;

public class ReconciliationQueryServiceTests
{
    [Fact]
    public async Task GetUserSnapshot_ReturnsOnlyReconciliationFields()
    {
        var user = new User(
            "grower.11",
            "password-hash-that-must-not-leak",
            "grower11@smartpalm.test",
            "Grower Eleven",
            UserRole.PalmGrower);
        SetId(user, 11);

        var subscription = new Subscription(
            user.Id,
            SubscriptionPlanCatalog.GetAll().First(),
            DateTime.UtcNow);
        subscription.Activate();

        var service = new ReconciliationQueryService(
            new StubUserRepository(user),
            new StubSubscriptionRepository(subscription));

        var result = await service.GetUserSnapshotAsync(11);
        var json = JsonSerializer.Serialize(result);

        Assert.NotNull(result);
        Assert.Equal(11, result!.userId);
        Assert.Equal("grower.11", result.username);
        Assert.Equal("PalmGrower", result.role);
        Assert.Equal("Active", result.status);
        Assert.Equal("Active", result.subscriptionStatus);
        Assert.DoesNotContain("PasswordHash", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password-hash-that-must-not-leak", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetUserSnapshot_ReturnsNullSubscriptionStatusWhenUserHasNoSubscription()
    {
        var user = new User(
            "grower.11",
            "password-hash",
            "grower11@smartpalm.test",
            "Grower Eleven",
            UserRole.PalmGrower);
        SetId(user, 11);

        var service = new ReconciliationQueryService(
            new StubUserRepository(user),
            new StubSubscriptionRepository(null));

        var result = await service.GetUserSnapshotAsync(11);

        Assert.NotNull(result);
        Assert.Null(result!.subscriptionStatus);
    }

    private static void SetId(User user, int id) =>
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, id);

    private sealed class StubUserRepository(User? user) : IUserRepository
    {
        public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(user?.Id == id ? user : null);

        public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            Task.FromResult<User?>(null);

        public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<User>>(Array.Empty<User>());

        public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<int> CountAdministratorsAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public void Remove(User user) { }
        public void Add(User user) { }
    }

    private sealed class StubSubscriptionRepository(Subscription? subscription) : ISubscriptionRepository
    {
        public Task<Subscription?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Subscription?>(null);

        public Task<Subscription?> FindByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(subscription?.UserId == userId ? subscription : null);

        public Task<IReadOnlyCollection<Subscription>> ListAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Subscription>>(Array.Empty<Subscription>());

        public void RemoveByUserId(int userId) { }
        public void Add(Subscription subscription) { }
    }
}
