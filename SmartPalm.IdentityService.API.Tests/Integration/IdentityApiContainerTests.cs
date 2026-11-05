using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SmartPalm.IdentityService.API.Tests.Integration;

public class IdentityApiContainerTests : IAsyncLifetime
{
    private const string AdminPassword = "Local-Test-Password-123!";
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("smartpalm_identity")
        .WithUsername("smartpalm_identity")
        .WithPassword("identity-tests")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4-management-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbitMq.DisposeAsync().AsTask());
    }

    [DockerRequiredFact]
    [Trait("Category", "Integration")]
    public async Task CreateUser_PersistsAggregateAndPublishesOutboxMessage()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:IdentityDatabase", _postgres.GetConnectionString());
            builder.UseSetting("Jwt:Issuer", "smartpalm.identity.tests");
            builder.UseSetting("Jwt:Audience", "smartpalm.clients.tests");
            builder.UseSetting("Jwt:Key", "identity-tests-signing-key-with-at-least-32-characters");
            builder.UseSetting("RabbitMq:Enabled", "true");
            builder.UseSetting("RabbitMq:Uri", _rabbitMq.GetConnectionString());
            builder.UseSetting("Seed:Enabled", "true");
            builder.UseSetting("Seed:AdminUsername", "admin");
            builder.UseSetting("Seed:AdminPassword", AdminPassword);
            builder.UseSetting("Seed:AdminEmail", "admin@smartpalm.test");
            builder.UseSetting("Seed:AdminFullName", "Integration Test Admin");
        });

        using var client = factory.CreateClient();
        var signIn = await client.PostAsJsonAsync("/api/v1/Authentication/sign-in", new
        {
            username = "admin",
            password = AdminPassword
        });
        signIn.EnsureSuccessStatusCode();
        var auth = await signIn.Content.ReadFromJsonAsync<JsonElement>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.GetProperty("token").GetString());

        var create = await client.PostAsJsonAsync("/api/v1/admin/users", new
        {
            username = "grower.integration",
            password = "Grower-Test-123!",
            email = "grower.integration@smartpalm.test",
            fullName = "Integration Grower",
            role = "PalmGrower"
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        var published = false;
        for (var attempt = 0; attempt < 10 && !published; attempt++)
        {
            await using var command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM identity.outbox_messages WHERE processed_on_utc IS NOT NULL",
                connection);
            published = Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
            if (!published) await Task.Delay(TimeSpan.FromSeconds(1));
        }

        Assert.True(published, "The outbox message was not published within the retry window.");
    }
}
