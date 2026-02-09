namespace Identity.API.Application.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task Handle(
        ChangePasswordCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, ct)
            ?? throw new IdentityDomainException("User not found");

        if (!passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Hash))
            throw new IdentityDomainException("Current password is incorrect");

        Password.ValidateStrength(command.NewPassword);

        var newPasswordHash = Password.FromHash(passwordHasher.Hash(command.NewPassword));
        user.ChangePassword(newPasswordHash);

        user.RevokeAllRefreshTokens();

        userRepository.Update(user);
        await userRepository.UnitOfWork.SaveEntitiesAsync(ct);
    }
}