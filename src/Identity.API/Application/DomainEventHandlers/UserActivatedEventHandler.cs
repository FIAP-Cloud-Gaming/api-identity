namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserActivatedEventHandler(
    ILogger<UserActivatedEventHandler> logger)
    : INotificationHandler<UserActivatedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogUserActivated =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(103, "UserActivated"),
            "Domain Event: User activated - {Email} ({UserId})");

    public Task Handle(UserActivatedDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserActivated(logger, domainEvent.Email, domainEvent.UserId, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Enviar email de boas-vindas
        // - Notificar equipe
        // - Publicar evento de integração

        return Task.CompletedTask;
    }
}
