using SmartPalm.IdentityService.Domain.Model.Entities;

namespace SmartPalm.IdentityService.Domain.Repositories;

public interface IUserDeletionRepository
{
    Task<UserDeletionOperation?> FindAsync(Guid operationId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserDeletionServiceResult>> ListResultsAsync(Guid operationId, CancellationToken cancellationToken = default);
    void Add(UserDeletionOperation operation);
    void AddResult(UserDeletionServiceResult result);
}
