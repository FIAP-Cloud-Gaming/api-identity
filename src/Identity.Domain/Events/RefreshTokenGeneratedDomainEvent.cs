namespace Identity.Domain.Events;

public record RefreshTokenGeneratedDomainEvent(int UserId, DateTime ExpiresAt) : INotification;
