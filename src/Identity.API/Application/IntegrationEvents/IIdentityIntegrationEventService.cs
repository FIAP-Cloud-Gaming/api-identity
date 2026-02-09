namespace Identity.API.Application.IntegrationEvents;

public interface IIdentityIntegrationEventService
{
    Task PublishEventsThroughEventBusAsync(Guid transactionId);
    Task AddAndSaveEventAsync(object @event);
}
