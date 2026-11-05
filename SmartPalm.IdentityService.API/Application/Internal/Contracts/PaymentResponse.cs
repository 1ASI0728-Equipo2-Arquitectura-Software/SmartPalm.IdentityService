namespace SmartPalm.IdentityService.Application.Contracts;

public record PaymentResponse(
    string planName,
    string periodStart,
    string periodEnd,
    decimal amount,
    string status,
    string processedAt);
