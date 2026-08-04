namespace TierMatch.Application.Authentication.DTOs;

public sealed record AdminUserListResponse(
    IReadOnlyCollection<AdminUserDto> Items,
    int TotalCount,
    int Page,
    int PageSize);