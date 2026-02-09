using Identity.API.Application.Commands.Identified;

namespace Identity.API.Application.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse>(
    IRequestManager requestManager,
    ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        if (!requestType.IsGenericType ||
            requestType.GetGenericTypeDefinition() != typeof(IdentifiedCommand<,>))
        {
            return await next(cancellationToken);
        }

        var requestIdProperty = requestType.GetProperty("Id");
        var commandProperty = requestType.GetProperty("Command");

        if (requestIdProperty == null || commandProperty == null)
        {
            return await next(cancellationToken);
        }

        var requestId = (Guid)requestIdProperty.GetValue(request)!;
        var command = commandProperty.GetValue(request);
        var commandName = command?.GetType().Name ?? typeof(TRequest).Name;

        var alreadyExists = await requestManager.ExistAsync(requestId);

        if (alreadyExists)
        {
            logger.LogInformation(
                "⚠️ Duplicate request detected. RequestId: {RequestId}, Command: {CommandName}",
                requestId,
                commandName);

            return CreateResultForDuplicateRequest();
        }

        await requestManager.CreateRequestForCommandAsync<TRequest>(requestId);

        logger.LogInformation(
            "✅ Processing idempotent command. RequestId: {RequestId}, Command: {CommandName}",
            requestId,
            commandName);

        return await next(cancellationToken);
    }

    private static TResponse CreateResultForDuplicateRequest()
    {
        if (typeof(TResponse) == typeof(bool))
        {
            return (TResponse)(object)true;
        }

        return default!;
    }
}

public class IdempotencyBehavior<TRequest>(
    IRequestManager requestManager,
    ILogger<IdempotencyBehavior<TRequest>> logger)
    : IPipelineBehavior<TRequest>
    where TRequest : notnull
{
    public async Task Handle(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var requestType = request.GetType();
        if (!requestType.IsGenericType ||
            requestType.GetGenericTypeDefinition() != typeof(IdentifiedCommand<>))
        {
            await next(cancellationToken);
            return;
        }

        var requestIdProperty = requestType.GetProperty("Id");
        var commandProperty = requestType.GetProperty("Command");

        if (requestIdProperty == null || commandProperty == null)
        {
            await next(cancellationToken);
            return;
        }

        var requestId = (Guid)requestIdProperty.GetValue(request)!;
        var command = commandProperty.GetValue(request);
        var commandName = command?.GetType().Name ?? typeof(TRequest).Name;

        var alreadyExists = await requestManager.ExistAsync(requestId);

        if (alreadyExists)
        {
            logger.LogInformation(
                "⚠️ Duplicate request detected. RequestId: {RequestId}, Command: {CommandName}",
                requestId,
                commandName);

            return;
        }

        await requestManager.CreateRequestForCommandAsync<TRequest>(requestId);

        logger.LogInformation(
            "✅ Processing idempotent command. RequestId: {RequestId}, Command: {CommandName}",
            requestId,
            commandName);

        await next(cancellationToken);
    }
}
