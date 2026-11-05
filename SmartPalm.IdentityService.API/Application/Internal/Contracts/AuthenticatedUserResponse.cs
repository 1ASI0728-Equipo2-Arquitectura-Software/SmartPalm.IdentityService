namespace SmartPalm.IdentityService.Application.Contracts;

public record AuthenticatedUserResponse(
    int UserId,
    string Username,
    string Email,
    string FullName,
    string Role,
    string Token);
