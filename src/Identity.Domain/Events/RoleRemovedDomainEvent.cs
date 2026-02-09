namespace Identity.Domain.Events;

public record RoleRemovedDomainEvent(int UserId, string RoleName) : INotification;
