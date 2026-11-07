namespace SmartPalm.IdentityService.Application.Contracts;

public record UserDeletionOperationResponse(
    Guid operationId,
    int? userId,
    string status,
    int expectedServices,
    int completedServices,
    DateTime requestedAt,
    DateTime? completedAt,
    string? error);
