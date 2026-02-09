namespace Identity.Domain.Events;

public record RefreshTokenRevokedDomainEvent(int UserId, string Reason) : INotification;
