using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Authentication.Interfaces;

public interface IIdentityService
{
    Task<Result<AuthenticationResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<AuthenticationResponse>> RefreshAsync(
        RefreshRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> LogoutAllAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<CurrentUserResponse>> UpdateCurrentUserProfileAsync(
        Guid userId,
        UpdateCurrentUserProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> AssignShelterAdminAsync(
        Guid userId,
        Guid shelterId,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveShelterAccessAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result> SetUserActiveStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<Result<AdminUserListResponse>> GetAdminUsersAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<Result<AdminUserDto>> GetAdminUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
