using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartPalm.IdentityService.Infrastructure.Configuration;
using SmartPalm.IdentityService.Infrastructure.Persistence;

namespace SmartPalm.IdentityService.Infrastructure.Messaging;

internal class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("RabbitMQ outbox publisher is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Outbox dispatch failed; pending messages will be retried.");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var messages = await context.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(50)
            .ToArrayAsync(cancellationToken);
        if (messages.Length == 0) return;

        var factory = new ConnectionFactory { Uri = new Uri(_options.Uri) };
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(
            _options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var properties = new BasicProperties
                {
                    Persistent = true,
                    MessageId = message.Id.ToString(),
                    Type = message.EventType,
                    ContentType = "application/json"
                };
                await channel.BasicPublishAsync(
                    exchange: _options.Exchange,
                    routingKey: message.EventType,
                    mandatory: false,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(message.Content),
                    cancellationToken: cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception exception)
            {
                message.RetryCount++;
                message.Error = exception.Message[..Math.Min(exception.Message.Length, 2000)];
                throw;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
