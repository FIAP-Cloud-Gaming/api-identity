namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class Role : EntityWithNotification
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    protected Role() { }

    public Role(string name, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new IdentityDomainException("Role name is required");

        Name = name.ToUpperInvariant();
        Description = description;
        IsSystem = isSystem;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string User = "USER";
        public const string Manager = "MANAGER";
    }
}
