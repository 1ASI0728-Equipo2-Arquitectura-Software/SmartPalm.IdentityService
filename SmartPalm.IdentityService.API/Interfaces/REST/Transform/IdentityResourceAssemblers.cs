using SmartPalm.IdentityService.Domain.Model.Commands;
using SmartPalm.IdentityService.Interfaces.REST.Resources;

namespace SmartPalm.IdentityService.Interfaces.REST.Transform;

public static class IdentityResourceAssemblers
{
    public static SignInCommand ToCommand(this SignInResource resource) =>
        new(resource.Username, resource.Password);

    public static CreateUserCommand ToCommand(this CreateUserResource resource) =>
        new(resource.username, resource.password, resource.email, resource.fullName, resource.role, resource.planType);

    public static CreateSubscriptionCommand ToCommand(this CreateSubscriptionResource resource) =>
        new(resource.UserId, resource.PlanType);

    public static ProcessPaymentCommand ToCommand(this ProcessPaymentResource resource, int userId) =>
        new(userId, resource.amount);
}
