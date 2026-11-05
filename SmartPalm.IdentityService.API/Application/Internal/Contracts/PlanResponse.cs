namespace SmartPalm.IdentityService.Application.Contracts;

public record PlanResponse(
    string type,
    string name,
    decimal price,
    string billingCycle,
    int? maxHectares,
    int? maxSensors,
    int? maxPlantationHistory);
