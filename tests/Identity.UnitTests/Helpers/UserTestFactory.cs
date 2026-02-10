namespace Identity.UnitTests.Helpers;

internal static class UserTestFactory
{
    public static User CreateValidUser(
        string email = "test@example.com",
        string firstName = "John",
        string lastName = "Doe",
        string passwordHash = "ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789-0123456789ABCDEF0123456789ABCDEF")
    {
        var emailVo = new Email(email);
        var password = Password.FromHash(passwordHash);
        var name = new PersonName(firstName, lastName);
        var user = User.Create(emailVo, password, name);
        user.ClearNotification();
        return user;
    }

    private static int _nextRoleId = 100;

    public static Role CreateRole(string name = "USER", string? description = null, bool isSystem = false)
    {
        var role = new Role(name, description, isSystem);
        // Set a unique Id via reflection so AssignRole deduplication works correctly in tests
        var idField = typeof(Role).BaseType!.BaseType!.GetField("_id",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        idField!.SetValue(role, Interlocked.Increment(ref _nextRoleId));
        return role;
    }

    /// <summary>
    /// Assigns a role to a user and populates the Role navigation property using reflection.
    /// This is needed for unit tests where EF Core doesn't automatically load navigation properties.
    /// </summary>
    public static void AssignRoleWithNavigation(User user, Role role)
    {
        user.AssignRole(role);

        // Use reflection to set the Role navigation property on the UserRole
        var userRoles = user.GetType()
            .GetProperty("UserRoles")!
            .GetValue(user) as IEnumerable<UserRole>;

        var userRole = userRoles!.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (userRole is not null)
        {
            var roleProperty = typeof(UserRole).GetProperty("Role");
            roleProperty!.SetValue(userRole, role);
        }
    }
}
