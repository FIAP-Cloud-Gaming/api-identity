namespace Identity.API.Application.IntegrationEvents.EventHandling;

internal sealed class PaymentSucceededIntegrationEventHandler(
    ILogger<PaymentSucceededIntegrationEventHandler> logger)
    : IIntegrationEventHandler<PaymentSucceededIntegrationEvent>
{
    public Task Handle(PaymentSucceededIntegrationEvent @event)
    {
        logger.LogInformation(
            "Integration Event: Payment {PaymentId} succeeded for user {UserId}. Amount: {TotalAmount} {Currency}",
            @event.PaymentId, @event.UserId, @event.TotalAmount, @event.Currency);

        // TODO: Atualizar historico de pagamentos do usuario

        return Task.CompletedTask;
    }
}
