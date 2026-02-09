using Identity.Infrastructure.IntegrationEvents;

namespace Identity.API.Application.IntegrationEvents;

public class IdentityIntegrationEventService(
    IdentityContext context,
    IIntegrationEventLogService eventLogService,
    ILogger<IdentityIntegrationEventService> logger) : IIdentityIntegrationEventService
{
    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        var pendingLogEvents = await eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        foreach (var logEvt in pendingLogEvents)
        {
            logger.LogInformation("Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt);

            try
            {
                await eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);

                // TODO: Aqui você publicaria no message bus (RabbitMQ, Azure Service Bus, etc)
                // await _eventBus.PublishAsync(logEvt);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error publishing integration event: {IntegrationEventId}", logEvt.EventId);
                await eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    public async Task AddAndSaveEventAsync(object @event)
    {
        logger.LogInformation("Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", @event.GetType().Name, @event);

        var transaction = context.GetCurrentTransaction() 
            ?? throw new InvalidOperationException("A transaction must be active to save integration events");

        await eventLogService.SaveEventAsync(@event, transaction.TransactionId);
    }
}
