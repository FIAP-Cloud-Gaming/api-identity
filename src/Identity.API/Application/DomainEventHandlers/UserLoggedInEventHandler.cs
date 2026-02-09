namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserLoggedInEventHandler(
    ILogger<UserLoggedInEventHandler> logger)
    : INotificationHandler<UserLoggedInDomainEvent>
{
    private static readonly Action<ILogger, string, int, DateTime, Exception?> LogUserLoggedIn =
        LoggerMessage.Define<string, int, DateTime>(LogLevel.Information, new EventId(102, "UserLoggedIn"),
            "Domain Event: User logged in - {Email} ({UserId}) at {LoginAt}");

    public Task Handle(UserLoggedInDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserLoggedIn(logger, domainEvent.Email, domainEvent.UserId, domainEvent.LoginAt, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Atualizar estatísticas de login
        // - Detectar login suspeito (localização, horário, etc)
        // - Enviar notificação de login

        return Task.CompletedTask;
    }
}
