namespace SmartPalm.IdentityService.Application.Contracts;

public record InactiveSubscriptionResponse(string status, string planName, decimal amountDue);
