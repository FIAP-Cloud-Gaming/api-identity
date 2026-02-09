using Microsoft.EntityFrameworkCore;

namespace Identity.API.Application.Queries.GetUserById;

internal sealed class GetUserByIdQueryHandler(IdentityContext context)
    : IQueryHandler<GetUserByIdQuery, UserViewModel?>
{
    public async Task<UserViewModel?> Handle(
        GetUserByIdQuery query,
        CancellationToken ct)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Id == query.UserId)
            .Select(u => new UserViewModel(
                u.Id,
                u.Email.Value,
                u.Name.FirstName,
                u.Name.LastName,
                u.Name.FullName,
                u.Status.ToString(),
                u.CreatedAt,
                u.LastLoginAt,
                u.UserRoles.Select(ur => ur.Role!.Name).ToList()))
            .FirstOrDefaultAsync(ct);
    }
}