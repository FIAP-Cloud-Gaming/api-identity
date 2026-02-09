namespace Identity.API.Application.DomainEventHandlers;

internal sealed class PasswordChangedEventHandler(
    ILogger<PasswordChangedEventHandler> logger)
    : INotificationHandler<PasswordChangedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogPasswordChanged =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(101, "PasswordChanged"),
            "Domain Event: Password changed - {Email} ({UserId})");

    public Task Handle(PasswordChangedDomainEvent domainEvent, CancellationToken ct)
    {
        LogPasswordChanged(logger, domainEvent.Email, domainEvent.UserId, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Enviar email de notificação
        // - Invalidar todos os tokens ativos
        // - Registrar auditoria de segurança

        return Task.CompletedTask;
    }
}
