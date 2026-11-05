namespace SmartPalm.IdentityService.API.Tests.Integration;

[AttributeUsage(AttributeTargets.Method)]
public class DockerRequiredFactAttribute : FactAttribute
{
    public DockerRequiredFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("RUN_TESTCONTAINERS"),
                "true",
                StringComparison.OrdinalIgnoreCase))
            Skip = "Set RUN_TESTCONTAINERS=true and start Docker to run container integration tests.";
    }
}
