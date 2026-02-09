namespace Identity.Infrastructure.Repositories;

public class UserRepository(IdentityContext context) : IUserRepository
{
    private readonly IdentityContext _context = context;
    public IUnitOfWork UnitOfWork => _context;


    public User Add(User user) 
        => _context.Users.Add(user).Entity;

    public User Update(User user) 
        => _context.Users.Update(user).Entity;

    public void Delete(User user) 
        => _context.Users.Remove(user);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default) 
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByIdWithRolesAsync(int id, CancellationToken ct = default) 
        => await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByIdWithTokensAsync(int id, CancellationToken ct = default) 
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.Value == normalizedEmail, ct);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) 
        => await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(rt => rt.Token == refreshToken &&
                                          rt.RevokedAt == null &&
                                          rt.ExpiresAt > DateTime.UtcNow), ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.Email.Value == normalizedEmail, ct);
    }

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default) 
        => await _context.Users
            .Where(u => u.Status == UserStatus.Active)
            .ToListAsync(ct);

    public async Task<List<User>> GetUsersByRoleAsync(string roleName, CancellationToken ct = default)
    {
        var normalizedRole = roleName.ToUpperInvariant();
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.UserRoles.Any(ur => ur.Role!.Name == normalizedRole))
            .ToListAsync(ct);
    }

    public async Task<Role?> GetRoleByIdAsync(int id, CancellationToken ct = default) 
        => await _context.Roles.FindAsync([id], ct);

    public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == normalizedName, ct);
    }

    public async Task<List<Role>> GetAllRolesAsync(CancellationToken ct = default) 
        => await _context.Roles.ToListAsync(ct);
}
