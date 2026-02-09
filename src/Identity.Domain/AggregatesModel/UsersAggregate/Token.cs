namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class Token : ValueObject
{
    public string Value { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    protected Token() { }

    public Token(string value, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new IdentityDomainException("Token value is required");

        if (expiresAt <= DateTime.UtcNow)
            throw new IdentityDomainException("Token expiration must be in the future");

        Value = value;
        ExpiresAt = expiresAt;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return ExpiresAt;
    }

    public static implicit operator string(Token token) => token.Value;
}
