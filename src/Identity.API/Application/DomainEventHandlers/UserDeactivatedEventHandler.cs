namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserDeactivatedEventHandler(
    IIdentityIntegrationEventService integrationEventService,
    ILogger<UserDeactivatedEventHandler> logger)
    : INotificationHandler<UserDeactivatedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogUserDeactivated =
        LoggerMessage.Define<string, int>(LogLevel.Warning, new EventId(104, "UserDeactivated"),
            "Domain Event: User deactivated - {Email} ({UserId})");

    public async Task Handle(UserDeactivatedDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserDeactivated(logger, domainEvent.Email, domainEvent.UserId, null);

        var integrationEvent = new UserDeactivatedIntegrationEvent(
            domainEvent.UserId, domainEvent.Email);

        await integrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
