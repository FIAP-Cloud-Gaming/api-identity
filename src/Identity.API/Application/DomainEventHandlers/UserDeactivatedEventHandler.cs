namespace Identity.API.Application.DomainEventHandlers;

internal sealed class UserDeactivatedEventHandler(
    ILogger<UserDeactivatedEventHandler> logger)
    : INotificationHandler<UserDeactivatedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogUserDeactivated =
        LoggerMessage.Define<string, int>(LogLevel.Warning, new EventId(104, "UserDeactivated"),
            "Domain Event: User deactivated - {Email} ({UserId})");

    public Task Handle(UserDeactivatedDomainEvent domainEvent, CancellationToken ct)
    {
        LogUserDeactivated(logger, domainEvent.Email, domainEvent.UserId, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Invalidar todos os tokens do usuário
        // - Revogar todas as sessões ativas
        // - Enviar email de notificação
        // - Publicar evento de integração para outros serviços

        return Task.CompletedTask;
    }
}
