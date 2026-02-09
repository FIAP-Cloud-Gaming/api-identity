namespace Identity.Infrastructure.Services;

public sealed class IdentityService(IHttpContextAccessor httpContextAccessor) : IIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public string? GetUserIdentity()
    {
        var subClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);

        if (subClaim != null)
            return subClaim.Value;

        var subDirectClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst("sub");

        return subDirectClaim?.Value;
    }

    public string? GetUserName() 
        => _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Name)?.Value;

    public string? GetUserEmail() 
        => _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated() 
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public IEnumerable<string> GetUserRoles()
    {
        var roles = _httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value);

        return roles ?? [];
    }
}