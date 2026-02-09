namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class Email : ValueObject
{
    public string Value { get; private set; } = null!;

    protected Email() { }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new IdentityDomainException("Email is required");

        if (!IsValidEmail(value))
            throw new IdentityDomainException("Invalid email format");

        Value = value.ToLowerInvariant().Trim();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}
