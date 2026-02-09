namespace Identity.API.Application.DomainEventHandlers;

internal sealed class RoleRemovedEventHandler(
    ILogger<RoleRemovedEventHandler> logger)
    : INotificationHandler<RoleRemovedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogRoleRemoved =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(106, "RoleRemoved"),
            "Domain Event: Role removed - {RoleName} from User ({UserId})");

    public Task Handle(RoleRemovedDomainEvent domainEvent, CancellationToken ct)
    {
        LogRoleRemoved(logger, domainEvent.RoleName, domainEvent.UserId, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Enviar notificação ao usuário
        // - Invalidar cache de permissões
        // - Revogar tokens com essa role específica
        // - Publicar evento de integração

        return Task.CompletedTask;
    }
}
