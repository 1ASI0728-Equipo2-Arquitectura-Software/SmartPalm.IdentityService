using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Repositories;

namespace SmartPalm.IdentityService.Application.Services;

public class UserDeletionQueryService(IUserDeletionRepository deletions)
{
    public async Task<UserDeletionOperationResponse?> GetAsync(
        Guid operationId,
        CancellationToken cancellationToken = default)
    {
        var operation = await deletions.FindAsync(operationId, cancellationToken);
        return operation is null
            ? null
            : new UserDeletionOperationResponse(
                operation.Id,
                operation.UserId,
                operation.Status.ToString(),
                operation.ExpectedServices,
                operation.CompletedServices,
                operation.RequestedAtUtc,
                operation.CompletedAtUtc,
                operation.Error);
    }
}
