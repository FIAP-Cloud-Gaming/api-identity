namespace Identity.API.Application.IntegrationEvents.Events;

public record UserRegisteredIntegrationEvent(int UserId, string Email, string FullName) : IntegrationEvent;
