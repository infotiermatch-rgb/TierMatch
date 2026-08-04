using Microsoft.EntityFrameworkCore;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;
using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public sealed partial class IdentityService
{
    public async Task<Result<AdminUserListResponse>>
        GetAdminUsersAsync(
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (page < 1)
        {
            return Result<AdminUserListResponse>.Validation(
                "Die Seitennummer muss mindestens 1 betragen.");
        }

        if (pageSize is < 1 or > 100)
        {
            return Result<AdminUserListResponse>.Validation(
                "Die Seitengröße muss zwischen 1 und 100 liegen.");
        }

        var query =
            _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch =
                search.Trim().ToLower();

            query = query.Where(user =>
                user.FirstName.ToLower()
                    .Contains(normalizedSearch) ||
                user.LastName.ToLower()
                    .Contains(normalizedSearch) ||
                (
                    user.Email != null &&
                    user.Email.ToLower()
                        .Contains(normalizedSearch)
                ));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var users = await query
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserDto>(
            users.Count);

        foreach (var user in users)
        {
            items.Add(
                await MapAdminUserAsync(user));
        }

        var response = new AdminUserListResponse(
            Items: items.AsReadOnly(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize);

        return Result<AdminUserListResponse>.Success(
            response);
    }

    public async Task<Result<AdminUserDto>>
        GetAdminUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            return Result<AdminUserDto>.Validation(
                "Es wurde keine gültige Benutzer-ID angegeben.");
        }

        var user = await _userManager.FindByIdAsync(
            userId.ToString());

        if (user is null)
        {
            return Result<AdminUserDto>.NotFound(
                "Benutzer wurde nicht gefunden.");
        }

        var dto = await MapAdminUserAsync(user);

        return Result<AdminUserDto>.Success(dto);
    }

    private async Task<AdminUserDto> MapAdminUserAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(
            user);

        var orderedRoles = roles
            .OrderBy(role => role)
            .ToList()
            .AsReadOnly();

        return new AdminUserDto(
            Id: user.Id,
            Email: user.Email ?? string.Empty,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Roles: orderedRoles,
            ShelterId: user.ShelterId,
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt);
    }
}