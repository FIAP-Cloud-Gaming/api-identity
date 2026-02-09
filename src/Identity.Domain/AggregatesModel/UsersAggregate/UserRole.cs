namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class UserRole : EntityWithNotification
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    public User? User { get; private set; }
    public Role? Role { get; private set; }

    protected UserRole() { }

    internal UserRole(int userId, int roleId)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }
}
