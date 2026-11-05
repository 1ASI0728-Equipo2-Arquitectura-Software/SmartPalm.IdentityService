using Microsoft.EntityFrameworkCore;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Subscriptions;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

internal class PaymentRepository(IdentityDbContext context) : IPaymentRepository
{
    public async Task<IReadOnlyCollection<PaymentTransaction>> ListByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await context.Payments.AsNoTracking()
            .Where(payment => payment.UserId == userId)
            .OrderByDescending(payment => payment.ProcessedAt)
            .ToArrayAsync(cancellationToken);

    public void Add(PaymentTransaction payment) => context.Payments.Add(payment);
}
