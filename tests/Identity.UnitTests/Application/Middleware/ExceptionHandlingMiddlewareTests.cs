using Identity.API.Middleware;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace Identity.UnitTests.Application.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_NoException_PassesThrough()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        var nextCalled = false;

        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_Returns400WithProblemDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/users/register";

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format"),
            new ValidationFailure("Password", "Password too weak")
        };

        RequestDelegate next = (ctx) => throw new ValidationException(validationFailures);

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Validation Error");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Contain("validation");
    }

    [Fact]
    public async Task InvokeAsync_IdentityDomainException_Returns400WithProblemDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/users/login";

        RequestDelegate next = (ctx) => throw new IdentityDomainException("Invalid credentials");

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(400);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Domain Error");
        problemDetails.GetProperty("status").GetInt32().Should().Be(400);
        problemDetails.GetProperty("detail").GetString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401WithProblemDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/users/profile";

        RequestDelegate next = (ctx) => throw new UnauthorizedAccessException();

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Unauthorized");
        problemDetails.GetProperty("status").GetInt32().Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_Returns500WithProblemDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/users";

        RequestDelegate next = (ctx) => throw new InvalidOperationException("Unexpected error");

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.GetProperty("title").GetString().Should().Be("Server Error");
        problemDetails.GetProperty("status").GetInt32().Should().Be(500);
        problemDetails.GetProperty("detail").GetString().Should().Contain("unexpected");
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_IncludesErrorsInExtensions()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format"),
            new ValidationFailure("Email", "Email already exists"),
            new ValidationFailure("Password", "Password too weak")
        };

        RequestDelegate next = (ctx) => throw new ValidationException(validationFailures);

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);

        problemDetails.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.TryGetProperty("Email", out var emailErrors).Should().BeTrue();
        errors.TryGetProperty("Password", out var passwordErrors).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AllExceptionTypes_IncludeInstancePath()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        var path = "/api/test/path";

        // Test ValidationException
        var context1 = new DefaultHttpContext();
        context1.Response.Body = new MemoryStream();
        context1.Request.Path = path;
        RequestDelegate next1 = (ctx) => throw new ValidationException(new[] { new ValidationFailure("Test", "Error") });
        var middleware1 = new ExceptionHandlingMiddleware(next1, logger);

        await middleware1.InvokeAsync(context1);
        context1.Response.Body.Position = 0;
        var response1 = await new StreamReader(context1.Response.Body).ReadToEndAsync();
        var problem1 = JsonSerializer.Deserialize<JsonElement>(response1);
        problem1.GetProperty("instance").GetString().Should().Be(path);

        // Test IdentityDomainException
        var context2 = new DefaultHttpContext();
        context2.Response.Body = new MemoryStream();
        context2.Request.Path = path;
        RequestDelegate next2 = (ctx) => throw new IdentityDomainException("Error");
        var middleware2 = new ExceptionHandlingMiddleware(next2, logger);

        await middleware2.InvokeAsync(context2);
        context2.Response.Body.Position = 0;
        var response2 = await new StreamReader(context2.Response.Body).ReadToEndAsync();
        var problem2 = JsonSerializer.Deserialize<JsonElement>(response2);
        problem2.GetProperty("instance").GetString().Should().Be(path);
    }

    [Fact]
    public async Task InvokeAsync_Exception_LogsError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
        logger.IsEnabled(LogLevel.Error).Returns(true);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Test exception");
        RequestDelegate next = (ctx) => throw exception;

        var middleware = new ExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
