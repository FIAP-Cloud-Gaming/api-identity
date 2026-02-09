namespace Identity.API.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand command,
        CancellationToken ct)
    {
        if (await userRepository.ExistsByEmailAsync(command.Email, ct))
            throw new IdentityDomainException("Email already registered");

        Password.ValidateStrength(command.Password);

        var email = new Email(command.Email);
        var passwordHash = Password.FromHash(passwordHasher.Hash(command.Password));
        var name = new PersonName(command.FirstName, command.LastName);

        var user = User.Create(email, passwordHash, name);

        var defaultRole = await userRepository.GetRoleByNameAsync("USER", ct);
        if (defaultRole is not null)
            user.AssignRole(defaultRole);

        userRepository.Add(user);
        await userRepository.UnitOfWork.SaveEntitiesAsync(ct);

        logger.LogInformation("User registered: {Email} ({UserId})",
            user.Email.Value, user.Id);

        return new RegisterUserResult(user.Id, user.Email.Value, user.Name.FullName);
    }
}