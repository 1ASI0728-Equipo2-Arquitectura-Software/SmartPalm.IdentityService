using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartPalm.IdentityService.Infrastructure.Persistence;

namespace SmartPalm.IdentityService.Api;

public class DatabaseHealthCheck(IdentityDbContext context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext healthCheckContext,
        CancellationToken cancellationToken = default) =>
        await context.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Identity database is unavailable.");
}
