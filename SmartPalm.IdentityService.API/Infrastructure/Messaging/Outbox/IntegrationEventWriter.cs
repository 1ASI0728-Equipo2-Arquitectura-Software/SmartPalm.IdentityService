using System.Text.Json;
using SmartPalm.IdentityService.Application.Abstractions;
using SmartPalm.IdentityService.Infrastructure.Persistence;

namespace SmartPalm.IdentityService.Infrastructure.Messaging;

internal class IntegrationEventWriter(IdentityDbContext context) : IIntegrationEventWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue<T>(string eventType, T payload)
    {
        var id = Guid.NewGuid();
        var occurredAtUtc = DateTime.UtcNow;
        var envelope = new
        {
            eventId = id,
            eventType,
            schemaVersion = 1,
            occurredAtUtc,
            correlationId = Guid.NewGuid(),
            causationId = (Guid?)null,
            producer = "smartpalm.identity-service",
            payload
        };

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
            EventType = eventType,
            Content = JsonSerializer.Serialize(envelope, SerializerOptions),
            OccurredOnUtc = occurredAtUtc
        });
    }
}
