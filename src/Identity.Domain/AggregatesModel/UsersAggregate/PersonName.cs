namespace Identity.Domain.AggregatesModel.UsersAggregate;

public class PersonName : ValueObject
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string FullName => $"{FirstName} {LastName}";

    protected PersonName() { }

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new IdentityDomainException("First name is required");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new IdentityDomainException("Last name is required");

        if (firstName.Length > 100)
            throw new IdentityDomainException("First name too long");

        if (lastName.Length > 100)
            throw new IdentityDomainException("Last name too long");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
