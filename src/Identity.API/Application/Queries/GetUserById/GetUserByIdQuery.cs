namespace Identity.API.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(int UserId) : IQuery<UserViewModel?>;