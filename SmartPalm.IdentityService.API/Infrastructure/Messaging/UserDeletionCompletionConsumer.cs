using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartPalm.IdentityService.Application.Services;
using SmartPalm.IdentityService.Domain.Events;
using SmartPalm.IdentityService.Infrastructure.Configuration;

namespace SmartPalm.IdentityService.Infrastructure.Messaging;

internal class UserDeletionCompletionConsumer(
    IServiceScopeFactory scopes,
    IOptions<RabbitMqOptions> options,
    ILogger<UserDeletionCompletionConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        if (!settings.Enabled) return;
        while (!token.IsCancellationRequested)
        {
            try { await ConsumeAsync(token); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "User deletion completion consumer disconnected; retrying.");
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory { Uri = new Uri(settings.Uri), AutomaticRecoveryEnabled = true };
        await using var connection = await factory.CreateConnectionAsync(token);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: token);
        await channel.ExchangeDeclareAsync(settings.Exchange, ExchangeType.Topic, true, false, cancellationToken: token);
        var queue = "smartpalm.identity.user-deletion-completions";
        await channel.QueueDeclareAsync(queue, true, false, false, cancellationToken: token);
        await channel.QueueBindAsync(queue, settings.Exchange, "smartpalm.identity.user-deletion-service-completed.v1", cancellationToken: token);
        await channel.BasicQosAsync(0, 10, false, token);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                using var document = JsonDocument.Parse(args.Body.ToArray());
                var payload = document.RootElement.TryGetProperty("payload", out var envelopePayload)
                    ? envelopePayload
                    : document.RootElement;
                var message = payload.Deserialize<UserDeletionServiceCompletedIntegrationEvent>(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    ?? throw new JsonException("Invalid user deletion completion payload.");
                using var scope = scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<UserDeletionCommandService>()
                    .RegisterServiceCompletionAsync(message, token);
                await channel.BasicAckAsync(args.DeliveryTag, false, token);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "User deletion completion event failed.");
                await channel.BasicNackAsync(args.DeliveryTag, false, true, token);
            }
        };

        await channel.BasicConsumeAsync(queue, false, consumer, token);
        await Task.Delay(Timeout.InfiniteTimeSpan, token);
    }
}
