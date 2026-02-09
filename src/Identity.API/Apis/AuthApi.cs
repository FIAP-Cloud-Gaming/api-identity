using Identity.API.Application.Commands.ChangePassword;
using Identity.API.Application.Commands.Identified;
using Identity.API.Application.Commands.Login;
using Identity.API.Application.Commands.RefreshToken;
using Identity.API.Application.Commands.RegisterUser;

namespace Identity.API.Apis;

public static class AuthApi
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account with email and password")
            .Produces<RegisterUserResult>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user")
            .WithDescription("Authenticates user and returns JWT access token + refresh token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Generates new access token using refresh token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        group.MapPut("/change-password", ChangePasswordAsync)
            .WithName("ChangePassword")
            .WithSummary("Change user password")
            .WithDescription("Changes password for authenticated user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("RequireAuthenticatedUser");

        group.MapGet("/validate", ValidateTokenAsync)
            .WithName("ValidateToken")
            .WithSummary("Validate JWT token")
            .WithDescription("Checks if current JWT token is valid and returns user info")
            .Produces<TokenValidationResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization("RequireAuthenticatedUser");

        return group;
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterUserCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return Results.Created($"/api/users/{result.UserId}", result);
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> ChangePasswordAsync(
        [FromBody] ChangePasswordCommand command,
        [FromHeader(Name = "x-requestid")] Guid? requestId,
        IMediator mediator,
        IIdentityService identityService,
        CancellationToken ct)
    {
        if (!requestId.HasValue || requestId.Value == Guid.Empty)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Missing Request ID",
                Detail = "Header 'x-requestid' is required for idempotent operations",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var userIdString = identityService.GetUserIdentity();

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            return Results.Unauthorized();

        var commandWithUserId = command with { UserId = userId };

        var identifiedCommand = new IdentifiedCommand<ChangePasswordCommand>(
            commandWithUserId,
            requestId.Value);

        await mediator.Send(identifiedCommand, ct);

        return Results.NoContent();
    }

    private static IResult ValidateTokenAsync(IIdentityService identityService)
    {
        if (!identityService.IsAuthenticated())
            return Results.Unauthorized();

        var response = new TokenValidationResponse(
            identityService.GetUserIdentity()!,
            identityService.GetUserName(),
            identityService.GetUserEmail(),
            identityService.GetUserRoles(),
            true
        );

        return Results.Ok(response);
    }
}
