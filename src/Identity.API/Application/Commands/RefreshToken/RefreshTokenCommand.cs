namespace Identity.API.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<LoginResponse>;