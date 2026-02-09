using System.Security.Claims;

namespace Identity.Domain.Services;

public interface ITokenProvider
{
    Token GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
