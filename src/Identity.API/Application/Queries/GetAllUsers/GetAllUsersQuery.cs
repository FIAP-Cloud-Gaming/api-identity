namespace Identity.API.Application.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null,
    string? Role = null) : IQuery<PagedResponse<UserListViewModel>>;