namespace Identity.API.Application.IntegrationEvents.Events;

public record UserActivatedIntegrationEvent(int UserId, string Email) : IntegrationEvent;
