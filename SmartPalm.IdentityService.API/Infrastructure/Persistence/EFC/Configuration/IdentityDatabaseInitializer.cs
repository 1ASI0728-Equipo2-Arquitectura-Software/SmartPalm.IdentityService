using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Domain.Users;
using SmartPalm.IdentityService.Infrastructure.Configuration;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

public static class IdentityDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentityDatabaseInitializer");

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                break;
            }
            catch (Exception exception) when (attempt < 10)
            {
                logger.LogWarning(exception, "Database migration attempt {Attempt} failed; retrying.", attempt);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
        if (!seed.Enabled) return;
        if (string.IsNullOrWhiteSpace(seed.AdminUsername) ||
            string.IsNullOrWhiteSpace(seed.AdminPassword) ||
            string.IsNullOrWhiteSpace(seed.AdminEmail))
            throw new InvalidOperationException(
                "Seed admin requires Seed:AdminUsername, Seed:AdminPassword and Seed:AdminEmail.");

        if (await context.Users.AnyAsync(user => user.Username == seed.AdminUsername, cancellationToken))
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        context.Users.Add(new User(
            seed.AdminUsername,
            hasher.Hash(seed.AdminPassword),
            seed.AdminEmail,
            seed.AdminFullName,
            UserRole.Administrator));
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Development administrator '{Username}' was created.", seed.AdminUsername);
    }
}
