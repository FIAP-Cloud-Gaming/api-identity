namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserRegisteredEventHandler(
    ILogger<UserRegisteredEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogUserRegistered =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(100, "UserRegistered"),
            "Domain Event: User registered - {Email} ({UserId})");

    public Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserRegistered(logger, domainEvent.Email, domainEvent.UserId, null);


        return Task.CompletedTask;
    }
}