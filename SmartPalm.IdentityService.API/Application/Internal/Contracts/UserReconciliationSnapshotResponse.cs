namespace SmartPalm.IdentityService.Application.Contracts;

public record UserReconciliationSnapshotResponse(
    int userId,
    string username,
    string email,
    string fullName,
    string role,
    string status,
    string? subscriptionStatus);
