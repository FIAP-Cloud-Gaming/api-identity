namespace Identity.API.Application.IntegrationEvents.Events;

public record UserDeactivatedIntegrationEvent(int UserId, string Email) : IntegrationEvent;
