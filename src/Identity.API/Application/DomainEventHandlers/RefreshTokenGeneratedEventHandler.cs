namespace Identity.API.Application.DomainEventHandlers;

internal sealed class RefreshTokenGeneratedEventHandler(
    ILogger<RefreshTokenGeneratedEventHandler> logger)
    : INotificationHandler<RefreshTokenGeneratedDomainEvent>
{
    private static readonly Action<ILogger, int, DateTime, Exception?> LogRefreshTokenGenerated =
        LoggerMessage.Define<int, DateTime>(LogLevel.Information, new EventId(107, "RefreshTokenGenerated"),
            "Domain Event: Refresh token generated - User ({UserId}), Expires at {ExpiresAt}");

    public Task Handle(RefreshTokenGeneratedDomainEvent domainEvent, CancellationToken ct)
    {
        LogRefreshTokenGenerated(logger, domainEvent.UserId, domainEvent.ExpiresAt, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Limpar tokens expirados do usuário
        // - Notificar sobre novo dispositivo/sessão
        // - Registrar auditoria de segurança

        return Task.CompletedTask;
    }
}
