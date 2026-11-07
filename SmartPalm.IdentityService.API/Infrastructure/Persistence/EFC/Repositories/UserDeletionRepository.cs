using Microsoft.EntityFrameworkCore;
using SmartPalm.IdentityService.Domain.Model.Entities;
using SmartPalm.IdentityService.Domain.Repositories;

namespace SmartPalm.IdentityService.Infrastructure.Persistence;

internal class UserDeletionRepository(IdentityDbContext context) : IUserDeletionRepository
{
    public Task<UserDeletionOperation?> FindAsync(Guid operationId, CancellationToken cancellationToken = default) =>
        context.UserDeletionOperations.SingleOrDefaultAsync(x => x.Id == operationId, cancellationToken);

    public Task<bool> HasPendingForUserAsync(int userId, CancellationToken cancellationToken = default) =>
        context.UserDeletionOperations.AnyAsync(
            x => x.UserId == userId && x.Status == Domain.Users.UserDeletionStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyCollection<UserDeletionServiceResult>> ListResultsAsync(
        Guid operationId,
        CancellationToken cancellationToken = default) =>
        await context.UserDeletionServiceResults.AsNoTracking()
            .Where(x => x.OperationId == operationId)
            .ToArrayAsync(cancellationToken);

    public void Add(UserDeletionOperation operation) => context.UserDeletionOperations.Add(operation);
    public void AddResult(UserDeletionServiceResult result) => context.UserDeletionServiceResults.Add(result);
}
