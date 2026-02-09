namespace Identity.Domain.AggregatesModel.UsersAggregate;

public interface IUserRepository : IRepository<User>
{
    User Add(User user);
    User Update(User user);
    void Delete(User user);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default);
    Task<User?> GetByIdWithTokensAsync(int id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersByRoleAsync(string roleName, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct = default);
    Task<List<Role>> GetAllRolesAsync(CancellationToken ct = default);
}