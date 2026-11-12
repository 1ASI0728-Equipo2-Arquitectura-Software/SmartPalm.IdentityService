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
        var createdUser = await create.Content.ReadFromJsonAsync<JsonElement>();
        var userId = createdUser.GetProperty("id").GetInt32();

        var missingSubscription = await client.GetAsync($"/api/v1/admin/subscriptions/users/{userId}");
        Assert.Equal(HttpStatusCode.NotFound, missingSubscription.StatusCode);

        var subscriptionCreate = await client.PostAsJsonAsync("/api/v1/admin/subscriptions", new
        {
            userId,
            planType = "Seed"
        });
        Assert.Equal(HttpStatusCode.Created, subscriptionCreate.StatusCode);

        var pendingSubscription = await client.GetFromJsonAsync<JsonElement>(
            $"/api/v1/admin/subscriptions/users/{userId}");
        Assert.Equal("Pending", pendingSubscription.GetProperty("status").GetString());

        var payment = await client.PostAsJsonAsync(
            $"/api/v1/admin/subscriptions/users/{userId}/payments",
            new { amount = 149m });
        Assert.Equal(HttpStatusCode.Created, payment.StatusCode);

        var activeSubscription = await client.GetFromJsonAsync<JsonElement>(
            $"/api/v1/admin/subscriptions/users/{userId}");
        Assert.Equal(userId, activeSubscription.GetProperty("userId").GetInt32());
        Assert.Equal("Active", activeSubscription.GetProperty("status").GetString());

        var cancel = await client.PostAsJsonAsync(
            "/api/v1/admin/subscriptions/cancel",
            new { userId });
        Assert.Equal(HttpStatusCode.OK, cancel.StatusCode);

        var cancelledSubscription = await client.GetFromJsonAsync<JsonElement>(
            $"/api/v1/admin/subscriptions/users/{userId}");
        Assert.Equal("Cancelled", cancelledSubscription.GetProperty("status").GetString());

        await using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();
        var published = false;
        var cancellationEvent = false;
        for (var attempt = 0; attempt < 10 && (!published || !cancellationEvent); attempt++)
        {
            await using var command = new NpgsqlCommand(
                """
                SELECT
                    COUNT(*) FILTER (WHERE processed_on_utc IS NOT NULL),
                    COUNT(*) FILTER (
                        WHERE event_type = 'smartpalm.identity.subscription-cancelled.v1'
                          AND content::jsonb -> 'payload' ->> 'isActive' = 'false')
                FROM identity.outbox_messages
                """,
                connection);
            await using var reader = await command.ExecuteReaderAsync();
            await reader.ReadAsync();
            published = reader.GetInt64(0) > 0;
            cancellationEvent = reader.GetInt64(1) > 0;
            if (!published || !cancellationEvent) await Task.Delay(TimeSpan.FromSeconds(1));
        }

        Assert.True(published, "The outbox message was not published within the retry window.");
        Assert.True(cancellationEvent, "The cancellation event did not publish an explicit inactive plan state.");
    }
}
