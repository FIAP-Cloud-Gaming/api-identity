namespace Identity.Infrastructure.Idempotency;

public sealed class RequestManager(IdentityContext context) : IRequestManager
{
    private readonly IdentityContext _context = context 
        ?? throw new ArgumentNullException(nameof(context));

    public async Task<bool> ExistAsync(Guid id)
    {
        var request = await _context.ClientRequests
            .FindAsync(id);

        return request is not null;
    }

    public async Task CreateRequestForCommandAsync<T>(Guid id)
    {
        var exists = await ExistAsync(id);

        if (exists)
        {
            throw new IdentityDomainException($"Request with ID {id} already exists");
        }

        var request = new ClientRequest
        {
            Id = id,
            Name = typeof(T).Name,
            Time = DateTime.UtcNow
        };

        await _context.ClientRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }
}