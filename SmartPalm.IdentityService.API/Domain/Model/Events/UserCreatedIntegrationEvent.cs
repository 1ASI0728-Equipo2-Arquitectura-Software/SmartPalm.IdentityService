namespace SmartPalm.IdentityService.Domain.Events;

public record UserCreatedIntegrationEvent(
    int UserId,
    string Username,
    string Email,
    string FullName,
    string Role,
    DateTime OccurredAtUtc);
