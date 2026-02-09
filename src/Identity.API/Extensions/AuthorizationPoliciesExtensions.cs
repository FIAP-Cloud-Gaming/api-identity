namespace Identity.API.Extensions;

public static class AuthorizationPoliciesExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAuthenticatedUser", policy =>
                policy.RequireAuthenticatedUser())

            .AddPolicy("RequireAdminRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("ADMIN");
            })

            .AddPolicy("CanManageUsers", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.IsInRole("ADMIN") ||
                    context.User.IsInRole("MANAGER"));
            })

            .AddPolicy("RequireUserRole", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.IsInRole("USER") ||
                    context.User.IsInRole("MANAGER") ||
                    context.User.IsInRole("ADMIN"));
            });

        return services;
    }
}
