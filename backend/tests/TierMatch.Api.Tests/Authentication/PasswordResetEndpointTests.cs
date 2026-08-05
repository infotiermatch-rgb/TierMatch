using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authorization;
using TierMatch.Infrastructure.Identity;

using Xunit;

namespace TierMatch.Api.Tests.Authentication;

[Collection(TestCollection.Name)]
public sealed class PasswordResetEndpointTests
    : IntegrationTestBase
{
    private const string CurrentPassword =
        "TierMatch-Test123!";

    private const string NewPassword =
        "TierMatch-Reset456!";

    private const string AnotherPassword =
        "TierMatch-Another789!";

    public PasswordResetEndpointTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await EnsureIdentityRolesAsync();
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_BadRequest_When_Email_Is_Empty()
    {
        // Arrange
        using var anonymousClient =
            CreateAnonymousClient();

        var request =
            new ForgotPasswordRequest(
                Email: "   ");

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/forgot-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_NoContent_When_Email_Is_Unknown()
    {
        // Arrange
        using var anonymousClient =
            CreateAnonymousClient();

        var request =
            new ForgotPasswordRequest(
                Email:
                    $"unknown-{Guid.NewGuid():N}@tiermatch.test");

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/forgot-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_Should_Return_NoContent_For_Deactivated_User()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Deaktivierter",
                lastName: "Benutzer",
                emailPrefix: "forgot-inactive-user");

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        using var anonymousClient =
            CreateAnonymousClient();

        var request =
            new ForgotPasswordRequest(
                Email: registeredUser.Email);

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/forgot-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_Email_Is_Empty()
    {
        // Arrange
        var request =
            new ResetPasswordRequest(
                Email: "   ",
                Token: "reset-token",
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_Token_Is_Empty()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Leerer",
                lastName: "Token",
                emailPrefix: "empty-reset-token");

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: "   ",
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                CurrentPassword);

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_NewPassword_Is_Empty()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Leeres",
                lastName: "Passwort",
                emailPrefix: "empty-reset-password");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: "   ");

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                CurrentPassword);

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_Token_Is_Invalid()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Ungültiger",
                lastName: "Token",
                emailPrefix: "invalid-reset-token");

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: "ungueltiger-reset-token",
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                CurrentPassword);

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_Email_Is_Unknown()
    {
        // Arrange
        var request =
            new ResetPasswordRequest(
                Email:
                    $"unknown-{Guid.NewGuid():N}@tiermatch.test",
                Token: "ungueltiger-reset-token",
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_NewPassword_Is_Too_Weak()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Schwaches",
                lastName: "Passwort",
                emailPrefix: "weak-reset-password");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: "abc");

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                CurrentPassword);

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_Should_Return_BadRequest_When_User_Is_Deactivated()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Deaktivierter",
                lastName: "Reset",
                emailPrefix: "inactive-reset-user");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Should_Reset_Password_Successfully()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Erfolgreicher",
                lastName: "Reset",
                emailPrefix: "successful-reset");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        // Act
        using var response =
            await SendResetPasswordAsync(
                request);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task ResetPassword_Should_Prevent_Login_With_Old_Password()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Altes",
                lastName: "Passwort",
                emailPrefix: "old-password-after-reset");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        using var resetResponse =
            await SendResetPasswordAsync(
                request);

        resetResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act
        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                CurrentPassword);

        // Assert
        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_Should_Allow_Login_With_New_Password()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Neues",
                lastName: "Passwort",
                emailPrefix: "new-password-after-reset");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        using var resetResponse =
            await SendResetPasswordAsync(
                request);

        resetResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act
        using var loginResponse =
            await SendLoginAsync(
                registeredUser.Email,
                NewPassword);

        // Assert
        var responseContent =
            await loginResponse.Content.ReadAsStringAsync();

        loginResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var authentication =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthenticationResponse>();

        authentication.Should().NotBeNull();

        authentication!.UserId.Should().Be(
            registeredUser.UserId);

        authentication.RefreshToken
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ResetPassword_Should_Revoke_All_Existing_RefreshTokens()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Refresh",
                lastName: "Reset",
                emailPrefix: "reset-revoke-refresh");

        var firstRefreshToken =
            registeredUser.RefreshToken;

        var secondAuthentication =
            await LoginUserAsync(
                registeredUser.Email,
                CurrentPassword);

        var secondRefreshToken =
            secondAuthentication.RefreshToken;

        firstRefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        secondRefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        secondRefreshToken.Should().NotBe(
            firstRefreshToken);

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var request =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        using var resetResponse =
            await SendResetPasswordAsync(
                request);

        resetResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act
        using var firstRefreshResponse =
            await SendRefreshAsync(
                firstRefreshToken);

        using var secondRefreshResponse =
            await SendRefreshAsync(
                secondRefreshToken);

        // Assert
        firstRefreshResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);

        secondRefreshResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_Should_Not_Allow_Token_To_Be_Used_Twice()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Einmaliger",
                lastName: "Token",
                emailPrefix: "single-use-reset-token");

        var resetToken =
            await GeneratePasswordResetTokenAsync(
                registeredUser.UserId);

        var firstRequest =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: NewPassword);

        using var firstResponse =
            await SendResetPasswordAsync(
                firstRequest);

        firstResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        var secondRequest =
            new ResetPasswordRequest(
                Email: registeredUser.Email,
                Token: resetToken,
                NewPassword: AnotherPassword);

        // Act
        using var secondResponse =
            await SendResetPasswordAsync(
                secondRequest);

        // Assert
        secondResponse.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        using var newPasswordLogin =
            await SendLoginAsync(
                registeredUser.Email,
                NewPassword);

        newPasswordLogin.StatusCode.Should().Be(
            HttpStatusCode.OK);

        using var anotherPasswordLogin =
            await SendLoginAsync(
                registeredUser.Email,
                AnotherPassword);

        anotherPasswordLogin.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    private async Task<AuthenticationResponse>
        RegisterUserAsync(
            string firstName,
            string lastName,
            string emailPrefix)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var uniqueValue =
            Guid.NewGuid().ToString("N");

        var request =
            new RegisterRequest(
                FirstName: firstName,
                LastName: lastName,
                Email:
                    $"{emailPrefix}-{uniqueValue}@tiermatch.test",
                Password: CurrentPassword);

        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/register",
                request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var authentication =
            await response.Content
                .ReadFromJsonAsync<AuthenticationResponse>();

        authentication.Should().NotBeNull();

        authentication!.UserId.Should().NotBe(
            Guid.Empty);

        authentication.Email.Should()
            .NotBeNullOrWhiteSpace();

        authentication.RefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        return authentication;
    }

    private async Task<AuthenticationResponse>
        LoginUserAsync(
            string email,
            string password)
    {
        using var response =
            await SendLoginAsync(
                email,
                password);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var authentication =
            await response.Content
                .ReadFromJsonAsync<AuthenticationResponse>();

        authentication.Should().NotBeNull();

        authentication!.RefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        return authentication;
    }

    private async Task<string>
        GeneratePasswordResetTokenAsync(
            Guid userId)
    {
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user =
            await userManager.FindByIdAsync(
                userId.ToString());

        user.Should().NotBeNull();

        var resetToken =
            await userManager
                .GeneratePasswordResetTokenAsync(
                    user!);

        resetToken.Should()
            .NotBeNullOrWhiteSpace();

        return resetToken;
    }

    private async Task<HttpResponseMessage>
        SendLoginAsync(
            string email,
            string password)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var request =
            new LoginRequest(
                Email: email,
                Password: password);

        return await anonymousClient.PostAsJsonAsync(
            "/api/v1/auth/login",
            request);
    }

    private async Task<HttpResponseMessage>
        SendResetPasswordAsync(
            ResetPasswordRequest request)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        return await anonymousClient.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            request);
    }

    private async Task<HttpResponseMessage>
        SendRefreshAsync(
            string refreshToken)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var request =
            new RefreshRequest(
                RefreshToken: refreshToken);

        return await anonymousClient.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            request);
    }

    private async Task SetActiveStatusAsAdminAsync(
        Guid userId,
        bool isActive)
    {
        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: isActive);

        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{userId}/active-status",
                request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
    }

    private async Task EnsureIdentityRolesAsync()
    {
        using var scope =
            Factory.Services.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<
                    RoleManager<IdentityRole<Guid>>>();

        await EnsureRoleExistsAsync(
            roleManager,
            Roles.Admin);

        await EnsureRoleExistsAsync(
            roleManager,
            Roles.ShelterAdmin);

        await EnsureRoleExistsAsync(
            roleManager,
            Roles.User);
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(
                roleName))
        {
            return;
        }

        var result =
            await roleManager.CreateAsync(
                new IdentityRole<Guid>(
                    roleName));

        result.Succeeded.Should().BeTrue(
            string.Join(
                Environment.NewLine,
                result.Errors.Select(
                    error => error.Description)));
    }
}