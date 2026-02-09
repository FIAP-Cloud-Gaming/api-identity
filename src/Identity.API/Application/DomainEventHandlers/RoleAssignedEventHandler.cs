namespace Identity.API.Application.DomainEventHandlers;

internal sealed class RoleAssignedEventHandler(
    ILogger<RoleAssignedEventHandler> logger)
    : INotificationHandler<RoleAssignedDomainEvent>
{
    private static readonly Action<ILogger, string, int, Exception?> LogRoleAssigned =
        LoggerMessage.Define<string, int>(LogLevel.Information, new EventId(105, "RoleAssigned"),
            "Domain Event: Role assigned - {RoleName} to User ({UserId})");

    public Task Handle(RoleAssignedDomainEvent domainEvent, CancellationToken ct)
    {
        LogRoleAssigned(logger, domainEvent.RoleName, domainEvent.UserId, null);

        // TODO: Aqui você pode adicionar lógica adicional, como:
        // - Enviar notificação ao usuário
        // - Invalidar cache de permissões
        // - Publicar evento de integração para atualizar outros serviços

        return Task.CompletedTask;
    }
}
