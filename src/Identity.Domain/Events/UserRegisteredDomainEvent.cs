namespace Identity.Domain.Events;

public record UserRegisteredDomainEvent(User User) : INotification
{
    public int UserId => User.Id;
    public string Email => User.Email.Value;
    public string FullName => User.Name.FullName;
}
