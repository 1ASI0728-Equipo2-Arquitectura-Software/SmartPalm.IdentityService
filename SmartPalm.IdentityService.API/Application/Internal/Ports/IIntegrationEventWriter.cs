namespace SmartPalm.IdentityService.Application.Abstractions;

public interface IIntegrationEventWriter
{
    void Enqueue<T>(string eventType, T payload);
}
