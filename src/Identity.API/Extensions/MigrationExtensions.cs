namespace Identity.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using IdentityContext dbContext =
            scope.ServiceProvider.GetRequiredService<IdentityContext>();

        dbContext.Database.Migrate();
    }
}
