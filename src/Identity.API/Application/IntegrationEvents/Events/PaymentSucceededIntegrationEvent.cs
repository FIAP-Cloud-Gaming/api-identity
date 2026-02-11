namespace Identity.API.Application.IntegrationEvents.Events;

public record PaymentSucceededIntegrationEvent(Guid PaymentId, int UserId, decimal Amount) : IntegrationEvent;
