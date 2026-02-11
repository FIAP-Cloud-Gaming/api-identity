namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserActivatedEventHandler(
    IIdentityIntegrationEventService integrationEventService,
    ILogger<UserActivatedEventHandler> logger)
    : INotificationHandler<UserActivatedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogUserActivated =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(103, "UserActivated"),
            "Domain Event: User activated - {Email} ({UserId})");

    public async Task Handle(UserActivatedDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserActivated(logger, domainEvent.Email, domainEvent.UserId, null);

        var integrationEvent = new UserActivatedIntegrationEvent(
            domainEvent.UserId, domainEvent.Email);

        await integrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
