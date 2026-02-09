using Identity.API.Application.Queries.GetAllUsers;
using Identity.API.Application.Queries.GetUserById;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Identity.API.Apis;

public static class UsersApi
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();
        
        group.MapGet("/me", GetCurrentUserAsync)
            .WithName("GetCurrentUser")
            .WithSummary("Get current authenticated user")
            .WithDescription("Returns profile information of the authenticated user")
            .Produces<UserViewModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireAuthenticatedUser");

        group.MapGet("/{id:int}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .WithDescription("Returns user information by ID (Admin only or own user)")
            .Produces<UserViewModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("RequireAuthenticatedUser");

        group.MapGet("/", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .WithDescription("Returns list of all users (Admin only)")
            .Produces<IEnumerable<UserViewModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("CanManageUsers");

        return group;
    }

    private static async Task<Results<Ok<UserViewModel>, UnauthorizedHttpResult, NotFound>>
        GetCurrentUserAsync(
            IIdentityService identityService,
            IMediator mediator,
            CancellationToken ct)
    {
        var userIdString = identityService.GetUserIdentity();

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return TypedResults.Unauthorized();

        var query = new GetUserByIdQuery(userId);
        var user = await mediator.Receive(query, ct);

        if (user is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(user);
    }

    private static async Task<Results<Ok<UserViewModel>, ForbidHttpResult, NotFound>>
        GetUserByIdAsync(
            int id,
            IIdentityService identityService,
            IMediator mediator,
            CancellationToken ct)
    {
        var userIdString = identityService.GetUserIdentity();

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var currentUserId))
            return TypedResults.Forbid();

        var isAdmin = identityService.IsInRole("ADMIN");
        var isOwnProfile = currentUserId == id;

        if (!isAdmin && !isOwnProfile)
            return TypedResults.Forbid();

        var query = new GetUserByIdQuery(id);
        var user = await mediator.Receive(query, ct);

        if (user is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(user);
    }

    private static async Task<Results<Ok<PagedResponse<UserListViewModel>>, ForbidHttpResult>>
        GetAllUsersAsync(
            IIdentityService identityService,
            IMediator mediator,
            CancellationToken ct)
    {
        if (!identityService.IsInRole("ADMIN"))
            return TypedResults.Forbid();

        var query = new GetAllUsersQuery();
        var users = await mediator.Receive(query, ct);

        return TypedResults.Ok(users);
    }
}