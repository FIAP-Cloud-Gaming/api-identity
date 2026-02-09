namespace Identity.Domain.Events;

public record UserDeactivatedDomainEvent(int UserId, string Email) : INotification;
