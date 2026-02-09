namespace Identity.API.Application.Models;

public record TokenValidationResponse(
    string? UserId,
    string? UserName,
    string? Email,
    IEnumerable<string> Roles,
    bool IsAuthenticated
);