namespace Identity.Domain.Services;

public interface IIdentityService
{
    string? GetUserIdentity();
    string? GetUserName();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsInRole(string role);
    IEnumerable<string> GetUserRoles();
}
