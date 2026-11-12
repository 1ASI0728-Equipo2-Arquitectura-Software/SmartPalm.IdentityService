using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Application.Contracts;
using SmartPalm.IdentityService.Domain.Events;
using SmartPalm.IdentityService.Domain.Model.Entities;
using SmartPalm.IdentityService.Domain.Repositories;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.Application.Services;

public class UserDeletionCommandService(
    IUserRepository users,
    ISubscriptionRepository subscriptions,
    IPaymentRepository payments,
    IUserDeletionRepository deletions,
    IIntegrationEventWriter events,
    IUnitOfWork unitOfWork)
{
    private const int ExpectedServices = 7;

    public async Task<UserDeletionOperationResponse> DeleteAsync(
        int userId,
        int requesterId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) throw new ArgumentException("User id must be positive.");
        if (userId == requesterId) throw new InvalidOperationException("Administrators cannot delete their own account.");
        if (await deletions.HasPendingForUserAsync(userId, cancellationToken))
            throw new InvalidOperationException("A deletion operation is already pending for this user.");

        var user = await users.FindByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        if (user.Role == UserRole.Administrator)
            throw new InvalidOperationException("Administrator accounts cannot be deleted from this workflow.");

        var operation = new UserDeletionOperation(userId, ExpectedServices);
        return await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            deletions.Add(operation);
            payments.RemoveByUserId(userId);
            subscriptions.RemoveByUserId(userId);
            users.Remove(user);
            deletions.AddResult(new UserDeletionServiceResult(operation.Id, "identity", 3));
            operation.RegisterCompletion(1);
            events.Enqueue(
                "smartpalm.identity.user-deletion-requested.v1",
                new UserDeletionRequestedIntegrationEvent(operation.Id, userId, DateTime.UtcNow));
            await unitOfWork.SaveChangesAsync(token);
            return ToResponse(operation);
        }, cancellationToken);
    }

    public async Task RegisterServiceCompletionAsync(
        UserDeletionServiceCompletedIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var operation = await deletions.FindAsync(message.OperationId, token);
            if (operation is null || operation.Status != UserDeletionStatus.Pending) return true;
            var results = await deletions.ListResultsAsync(operation.Id, token);
            if (results.Any(x => x.ServiceName == message.ServiceName)) return true;
            deletions.AddResult(new UserDeletionServiceResult(
                message.OperationId, message.ServiceName, message.DeletedRecords));
            operation.RegisterCompletion(results.Count + 1);
            if (results.Count + 1 >= operation.ExpectedServices)
                operation.MarkCompleted(results.Count + 1);
            await unitOfWork.SaveChangesAsync(token);
            return true;
        }, cancellationToken);
    }

    private static UserDeletionOperationResponse ToResponse(UserDeletionOperation operation) => new(
        operation.Id,
        operation.UserId,
        operation.Status.ToString(),
        operation.ExpectedServices,
        operation.CompletedServices,
        operation.RequestedAtUtc,
        operation.CompletedAtUtc,
        operation.Error);
}
