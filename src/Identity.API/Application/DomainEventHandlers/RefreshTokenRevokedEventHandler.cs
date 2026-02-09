namespace Identity.API.Application.DomainEventHandlers;

internal sealed class RefreshTokenRevokedEventHandler(
    ILogger<RefreshTokenRevokedEventHandler> logger)
    : INotificationHandler<RefreshTokenRevokedDomainEvent>
{
    private static readonly Action<ILogger, int, string, Exception?> LogRefreshTokenRevoked =
        LoggerMessage.Define<int, string>(LogLevel.Information, new EventId(108, "RefreshTokenRevoked"),
            "Domain Event: Refresh token revoked - User ({UserId}), Reason: {Reason}");

    public Task Handle(RefreshTokenRevokedDomainEvent domainEvent, CancellationToken ct)
    {
        LogRefreshTokenRevoked(logger, domainEvent.UserId, domainEvent.Reason, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Registrar logout/revogação de sessão
        // - Limpar cache relacionado ao token
        // - Publicar evento para invalidar em cache distribuído
        // - Se reason for suspeito, alertar equipe de segurança

        return Task.CompletedTask;
    }
}
