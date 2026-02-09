namespace Identity.API.Application.Models;

public sealed record LoginResponse(
    int UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiration,
    IReadOnlyList<string> Roles);