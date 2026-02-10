using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure;

public class IdentityContextDesignTimeFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=IdentityDB;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName);
            });

        return new IdentityContext(optionsBuilder.Options, null!);
    }
}
