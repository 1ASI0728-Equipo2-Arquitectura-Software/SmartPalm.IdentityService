namespace SmartPalm.IdentityService.Application.Contracts;

public record AdminSubscriptionResponse(
    int userId,
    string planType,
    string planName,
    decimal price,
    string status,
    string startDate,
    string endDate,
    string billingCycle,
    string createdAt);
