namespace SmartPalm.IdentityService.Domain.Model.Commands;

public record CreateUserCommand(
    string Username,
    string Password,
    string Email,
    string FullName,
    string Role,
    string? PlanType = null);
