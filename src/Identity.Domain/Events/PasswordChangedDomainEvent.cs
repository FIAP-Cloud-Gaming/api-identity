namespace Identity.Domain.Events;

public record PasswordChangedDomainEvent(int UserId, string Email) : INotification;
