namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class Password : ValueObject
{
    public string Hash { get; private set; } = null!;

    protected Password() { }

    private Password(string hash)
    {
        Hash = hash;
    }

    public static Password FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new IdentityDomainException("Password hash is required");

        return new Password(hash);
    }

    public static void ValidateStrength(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new IdentityDomainException("Password is required");

        if (plainPassword.Length < 8)
            throw new IdentityDomainException("Password must be at least 8 characters");

        if (!plainPassword.Any(char.IsUpper))
            throw new IdentityDomainException("Password must contain uppercase letter");

        if (!plainPassword.Any(char.IsLower))
            throw new IdentityDomainException("Password must contain lowercase letter");

        if (!plainPassword.Any(char.IsDigit))
            throw new IdentityDomainException("Password must contain a number");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Hash;
    }

    public override string ToString() => "********";
}
