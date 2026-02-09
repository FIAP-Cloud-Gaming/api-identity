namespace Identity.API.Application.Models;

public sealed record UserViewModel(
    int Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<string> Roles);

public sealed record UserListViewModel(
    int Id,
    string Email,
    string FullName,
    string Status,
    DateTime CreatedAt);