namespace SmartPalm.IdentityService.Domain.Model.Commands;

public record ProcessPaymentCommand(int UserId, decimal Amount);
