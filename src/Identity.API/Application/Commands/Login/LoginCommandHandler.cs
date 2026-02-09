namespace Identity.API.Application.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        LoginCommand command,
        CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, ct)
            ?? throw new IdentityDomainException("Invalid email or password");

        if (user.Status != UserStatus.Active)
            throw new IdentityDomainException("User account is not active");

        if (!passwordHasher.Verify(command.Password, user.PasswordHash.Hash))
            throw new IdentityDomainException("Invalid email or password");

        var roles = user.UserRoles
            .Select(ur => ur.Role!.Name)
            .ToList();

        var accessToken = tokenProvider.GenerateAccessToken(user, roles);
        var refreshTokenValue = tokenProvider.GenerateRefreshToken();

        user.GenerateRefreshToken(refreshTokenValue, expirationDays: 7);
        user.RecordLogin();

        userRepository.Update(user);
        await userRepository.UnitOfWork.SaveEntitiesAsync(ct);

        logger.LogInformation("User logged in: {Email}", user.Email.Value);

        return new LoginResponse(
            user.Id,
            user.Email.Value,
            user.Name.FullName,
            accessToken.Value,
            refreshTokenValue,
            accessToken.ExpiresAt,
            roles);
    }
}