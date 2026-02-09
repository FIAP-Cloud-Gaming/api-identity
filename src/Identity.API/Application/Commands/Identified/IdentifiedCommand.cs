namespace Identity.API.Application.Commands.Identified;

public sealed class IdentifiedCommand<TCommand, TResponse>(TCommand command, Guid id) 
    : ICommand<TResponse> where TCommand 
        : ICommand<TResponse>
{
    public TCommand Command { get; } = command;
    public Guid Id { get; } = id;
}

public sealed class IdentifiedCommand<TCommand>(TCommand command, Guid id)
    : ICommand where TCommand
        : ICommand
{
    public TCommand Command { get; } = command;
    public Guid Id { get; } = id;
}