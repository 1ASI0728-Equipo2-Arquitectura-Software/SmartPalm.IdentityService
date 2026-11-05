namespace SmartPalm.IdentityService.Infrastructure.Configuration;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public bool Enabled { get; init; }
    public string Uri { get; init; } = "amqp://guest:guest@localhost:5672/";
    public string Exchange { get; init; } = "smartpalm.events";
}
