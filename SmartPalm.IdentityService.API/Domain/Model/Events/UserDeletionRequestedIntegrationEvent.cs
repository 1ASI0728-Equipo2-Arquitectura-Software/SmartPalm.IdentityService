namespace SmartPalm.IdentityService.Domain.Events;

public record UserDeletionRequestedIntegrationEvent(
    Guid OperationId,
    int UserId,
    DateTime OccurredAtUtc);
