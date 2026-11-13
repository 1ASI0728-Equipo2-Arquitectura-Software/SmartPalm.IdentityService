using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.IdentityService.Api.Controllers;

namespace SmartPalm.IdentityService.API.Tests.Interfaces.Rest;

public class ReconciliationControllerTests
{
    [Fact]
    public void SnapshotEndpoint_IsRestrictedToAdministrators()
    {
        var controller = typeof(ReconciliationController);
        var authorization = controller.GetCustomAttribute<AuthorizeAttribute>();
        var route = controller.GetCustomAttribute<RouteAttribute>();
        var action = controller.GetMethod(nameof(ReconciliationController.GetUserSnapshot));
        var httpGet = action?.GetCustomAttribute<HttpGetAttribute>();

        Assert.NotNull(authorization);
        Assert.Equal("Administrator", authorization!.Roles);
        Assert.Equal("api/v1/reconciliation", route?.Template);
        Assert.Equal("users/{userId:int}/snapshot", httpGet?.Template);
    }
}
