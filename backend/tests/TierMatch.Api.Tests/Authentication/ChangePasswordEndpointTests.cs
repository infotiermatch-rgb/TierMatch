using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authorization;

using Xunit;

namespace TierMatch.Api.Tests.Authentication;

[Collection(TestCollection.Name)]
public sealed class ChangePasswordEndpointTests
    : IntegrationTestBase
{
    private const string CurrentPassword =
        "TierMatch-Test123!";

    private const string NewPassword =
        "TierMatch-New456!";

    public ChangePasswordEndpointTests(
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
    public async Task ChangePassword_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        AuthenticateAsAnonymous();

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_BadRequest_When_CurrentPassword_Is_Wrong()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Falsches",
                lastName: "Passwort",
                emailPrefix: "wrong-current-password");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: "Falsches-Passwort123!",
                NewPassword: NewPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
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
    public async Task ChangePassword_Should_Return_BadRequest_When_NewPassword_Equals_CurrentPassword()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Gleiches",
                lastName: "Passwort",
                emailPrefix: "same-password");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: CurrentPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
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
    public async Task ChangePassword_Should_Return_BadRequest_When_NewPassword_Is_Too_Weak()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Schwaches",
                lastName: "Passwort",
                emailPrefix: "weak-password");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: "abc");

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
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
    public async Task ChangePassword_Should_Change_Password_Successfully()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Neues",
                lastName: "Passwort",
                emailPrefix: "successful-password-change");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task ChangePassword_Should_Prevent_Login_With_Old_Password()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Altes",
                lastName: "Login",
                emailPrefix: "old-password-login");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        using var changeResponse =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        changeResponse.StatusCode.Should().Be(
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
    public async Task ChangePassword_Should_Allow_Login_With_New_Password()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Neues",
                lastName: "Login",
                emailPrefix: "new-password-login");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        using var changeResponse =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        changeResponse.StatusCode.Should().Be(
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

        authentication.RefreshToken.Should()
            .NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ChangePassword_Should_Revoke_All_Existing_RefreshTokens()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Refresh",
                lastName: "Token",
                emailPrefix: "revoke-refresh-tokens");

        /*
         * Der erste Refresh Token stammt aus der Registrierung.
         */
        var firstRefreshToken =
            registeredUser.RefreshToken;

        /*
         * Durch eine weitere Anmeldung entsteht eine zweite,
         * unabhängige Sitzung.
         */
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

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        using var changeResponse =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        changeResponse.StatusCode.Should().Be(
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
    public async Task ChangePassword_Should_Return_Forbidden_When_User_Is_Deactivated()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync(
                firstName: "Deaktivierter",
                lastName: "Benutzer",
                emailPrefix: "inactive-password-change");

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_Should_Return_Unauthorized_When_User_Does_Not_Exist()
    {
        // Arrange
        AuthenticateAsUser(
            Guid.NewGuid());

        var request =
            new ChangePasswordRequest(
                CurrentPassword: CurrentPassword,
                NewPassword: NewPassword);

        // Act
        using var response =
            await Client.PostAsJsonAsync(
                "/api/v1/auth/change-password",
                request);

        // Assert
        response.StatusCode.Should().Be(
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

    private async Task<HttpResponseMessage>
        SendLoginAsync(
            string email,
            string password)
    {
        var anonymousClient =
            CreateAnonymousClient();

        var request =
            new LoginRequest(
                Email: email,
                Password: password);

        var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/login",
                request);

        anonymousClient.Dispose();

        return response;
    }

    private async Task<HttpResponseMessage>
        SendRefreshAsync(
            string refreshToken)
    {
        var anonymousClient =
            CreateAnonymousClient();

        var request =
            new RefreshRequest(
                RefreshToken: refreshToken);

        var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/refresh",
                request);

        anonymousClient.Dispose();

        return response;
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