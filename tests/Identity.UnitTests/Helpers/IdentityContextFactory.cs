namespace Identity.UnitTests.Helpers;

internal static class IdentityContextFactory
{
    public static IdentityContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<IdentityContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var mediator = Substitute.For<IMediator>();
        return new IdentityContext(options, mediator);
    }
}
