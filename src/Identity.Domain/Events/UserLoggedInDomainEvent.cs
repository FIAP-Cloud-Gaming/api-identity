namespace Identity.Domain.Events;

public record UserLoggedInDomainEvent(int UserId, string Email, DateTime LoginAt) : INotification;
