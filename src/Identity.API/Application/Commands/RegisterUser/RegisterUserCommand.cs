namespace Identity.API.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult(
    int UserId,
    string Email,
    string FullName);