namespace SmartPalm.IdentityService.Interfaces.REST.Resources;

public record CreateUserResource(
    string username,
    string password,
    string email,
    string fullName,
    string role,
    string? planType = null);
