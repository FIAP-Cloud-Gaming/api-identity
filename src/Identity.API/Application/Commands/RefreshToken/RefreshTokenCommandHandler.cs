namespace Identity.API.Application.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenProvider tokenProvider)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetByRefreshTokenAsync(command.RefreshToken, ct)
            ?? throw new IdentityDomainException("Invalid refresh token");

        user.RevokeRefreshToken(command.RefreshToken);

        var roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();
        var accessToken = tokenProvider.GenerateAccessToken(user, roles);
        var newRefreshToken = tokenProvider.GenerateRefreshToken();

        user.GenerateRefreshToken(newRefreshToken, expirationDays: 7);

        userRepository.Update(user);
        await userRepository.UnitOfWork.SaveEntitiesAsync(ct);

        return new LoginResponse(
            user.Id,
            user.Email.Value,
            user.Name.FullName,
            accessToken.Value,
            newRefreshToken,
            accessToken.ExpiresAt,
            roles);
    }
}