namespace SmartPalm.IdentityService.Domain.Model.Commands;

public record CreateSubscriptionCommand(int UserId, string PlanType);
