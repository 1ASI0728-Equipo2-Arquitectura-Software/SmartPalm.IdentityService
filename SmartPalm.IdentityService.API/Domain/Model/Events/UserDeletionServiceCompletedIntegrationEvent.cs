namespace SmartPalm.IdentityService.Domain.Events;

public record UserDeletionServiceCompletedIntegrationEvent(
    Guid OperationId,
    int UserId,
    string ServiceName,
    int DeletedRecords,
    DateTime OccurredAtUtc);
