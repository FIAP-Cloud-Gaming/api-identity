using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Infrastructure.Idempotency;

public sealed class RequestManagerTests : IDisposable
{
    private readonly IdentityContext _context;
    private readonly IRequestManager _requestManager;

    public RequestManagerTests()
    {
        _context = IdentityContextFactory.Create();
        _requestManager = new RequestManager(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Act
        var act = () => new RequestManager(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context");
    }

    [Fact]
    public async Task ExistAsync_ShouldReturnFalse_WhenRequestDoesNotExist()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        var exists = await _requestManager.ExistAsync(requestId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistAsync_ShouldReturnTrue_WhenRequestExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var request = new ClientRequest
        {
            Id = requestId,
            Name = "TestCommand",
            Time = DateTime.UtcNow
        };
        await _context.ClientRequests.AddAsync(request);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _requestManager.ExistAsync(requestId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRequestForCommandAsync_ShouldCreateRequest_WhenRequestDoesNotExist()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        await _requestManager.CreateRequestForCommandAsync<TestCommand>(requestId);

        // Assert
        var request = await _context.ClientRequests.FindAsync(requestId);
        request.Should().NotBeNull();
        request!.Id.Should().Be(requestId);
        request.Name.Should().Be("TestCommand");
        request.Time.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateRequestForCommandAsync_ShouldThrowIdentityDomainException_WhenRequestAlreadyExists()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        await _requestManager.CreateRequestForCommandAsync<TestCommand>(requestId);

        // Act
        var act = async () => await _requestManager.CreateRequestForCommandAsync<TestCommand>(requestId);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage($"Request with ID {requestId} already exists");
    }

    [Fact]
    public async Task CreateRequestForCommandAsync_ShouldSaveRequestToDatabase()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        await _requestManager.CreateRequestForCommandAsync<AnotherTestCommand>(requestId);

        // Assert
        var savedRequest = await _context.ClientRequests.FindAsync(requestId);
        savedRequest.Should().NotBeNull();
        savedRequest!.Name.Should().Be("AnotherTestCommand");
    }

    [Fact]
    public async Task CreateRequestForCommandAsync_ShouldUseTypeNameAsRequestName()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        await _requestManager.CreateRequestForCommandAsync<MyCustomCommand>(requestId);

        // Assert
        var request = await _context.ClientRequests.FindAsync(requestId);
        request.Should().NotBeNull();
        request!.Name.Should().Be("MyCustomCommand");
    }

    private sealed class TestCommand { }
    private sealed class AnotherTestCommand { }
    private sealed class MyCustomCommand { }
}
