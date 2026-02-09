namespace Identity.Infrastructure;

internal static class Schema
{
    public const string Identity = "identity";
}

/// <remark>
/// dotnet ef migrations add --startup-project ..\Identity.API --context IdentityContext [migration-name]
/// </remark>
public class IdentityContext(
    DbContextOptions<IdentityContext> options,
    IMediator mediator)
        : DbContext(options), IUnitOfWork, INotificationContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<ClientRequest> ClientRequests { get; set; } = null!;
    public DbSet<IntegrationEventLogEntry> IntegrationEventLogs { get; set; } = null!;

    private IDbContextTransaction? _currentTransaction;

    public IDbContextTransaction? GetCurrentTransaction() => _currentTransaction;

    public bool HasActiveTransaction => _currentTransaction != null;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema.Identity);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityContext).Assembly);
        modelBuilder.Entity<Role>().HasData(
              new { Id = 1, Name = "ADMIN", Description = "Administrator", IsSystem = true },
              new { Id = 2, Name = "USER", Description = "Regular User", IsSystem = true },
              new { Id = 3, Name = "MANAGER", Description = "Manager", IsSystem = true }
          );

        base.OnModelCreating(modelBuilder);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await mediator.DispatchNotificationsAsync(this, cancellationToken);

        return result > 0;
    }

    public IEnumerable<EntityWithNotification> GetEntitiesWithNotifications()
        => [.. ChangeTracker
            .Entries<EntityWithNotification>()
            .Where(entry => entry.Entity.Notifications.Count != 0)
            .Select(entry => entry.Entity)];

    public async Task<IDbContextTransaction?> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);

        if (transaction != _currentTransaction)
            throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }
    }
}
