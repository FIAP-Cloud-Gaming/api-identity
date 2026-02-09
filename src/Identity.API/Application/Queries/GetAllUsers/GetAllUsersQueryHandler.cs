namespace Identity.API.Application.Queries.GetAllUsers;

internal sealed class GetAllUsersQueryHandler(IdentityContext context)
    : IQueryHandler<GetAllUsersQuery, PagedResponse<UserListViewModel>>
{
    public async Task<PagedResponse<UserListViewModel>> Handle(
        GetAllUsersQuery query,
        CancellationToken ct)
    {
        var usersQuery = context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(query.Status) &&
            Enum.TryParse<UserStatus>(query.Status, out var status))
        {
            usersQuery = usersQuery.Where(u => u.Status == status);
        }

        if (!string.IsNullOrEmpty(query.Role))
        {
            var normalizedRole = query.Role.ToUpperInvariant();
            usersQuery = usersQuery.Where(u =>
                u.UserRoles.Any(ur => ur.Role!.Name == normalizedRole));
        }

        var totalCount = await usersQuery.CountAsync(ct);

        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserListViewModel(
                u.Id,
                u.Email.Value,
                u.Name.FullName,
                u.Status.ToString(),
                u.CreatedAt))
            .ToListAsync(ct);

        return new PagedResponse<UserListViewModel>(
            users, query.Page, query.PageSize, totalCount);
    }
}