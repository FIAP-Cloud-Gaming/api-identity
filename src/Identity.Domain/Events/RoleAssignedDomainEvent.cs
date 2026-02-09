namespace Identity.Domain.Events;

public record RoleAssignedDomainEvent(int UserId, string RoleName) : INotification;
