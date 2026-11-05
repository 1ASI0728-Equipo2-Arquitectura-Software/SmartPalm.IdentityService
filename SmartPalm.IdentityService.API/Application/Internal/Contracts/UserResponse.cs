namespace SmartPalm.IdentityService.Application.Contracts;

public record UserResponse(
    int id,
    string username,
    string email,
    string fullName,
    string role,
    string status);
