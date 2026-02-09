namespace Identity.Domain.Events;

public record UserActivatedDomainEvent(int UserId, string Email) : INotification;

