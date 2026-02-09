namespace Identity.API.Application.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword) : ICommand;