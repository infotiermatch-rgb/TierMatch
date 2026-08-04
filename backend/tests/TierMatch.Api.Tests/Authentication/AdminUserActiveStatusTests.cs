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
public sealed class AdminUserActiveStatusTests
    : IntegrationTestBase
{
    private const string TestPassword =
        "TierMatch-Test123!";

    public AdminUserActiveStatusTests(
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
    public async Task SetActiveStatus_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        AuthenticateAsAnonymous();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetActiveStatus_Should_Return_Forbidden_When_User_Has_User_Role()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        AuthenticateAsUser();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SetActiveStatus_Should_Return_Forbidden_When_User_Is_ShelterAdmin()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        AuthenticateAsShelterAdmin(
            Guid.NewGuid());

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        var missingUserId =
            Guid.NewGuid();

        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{missingUserId}/active-status",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Admin_Should_Not_Deactivate_Own_Account()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        /*
         * Der Test-Authentifizierungsmechanismus weist dem
         * aufrufenden Benutzer die Admin-Rolle und gleichzeitig
         * die ID des Zielbenutzers zu.
         */
        AuthenticateAsAdmin(
            registeredUser.UserId);

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Conflict);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Admin_Should_Deactivate_User()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivated_User_Should_Not_Be_Able_To_Login()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        using var anonymousClient =
            CreateAnonymousClient();

        var loginRequest = new
        {
            email = registeredUser.Email,
            password = TestPassword
        };

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/login",
                loginRequest);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deactivating_User_Should_Revoke_Existing_Refresh_Token()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        using var anonymousClient =
            CreateAnonymousClient();

        var refreshRequest = new
        {
            refreshToken =
                registeredUser.RefreshToken
        };

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/refresh",
                refreshRequest);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Should_Reactivate_User()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: true);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivated_User_Should_Be_Able_To_Login()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: true);

        using var anonymousClient =
            CreateAnonymousClient();

        var loginRequest = new
        {
            email = registeredUser.Email,
            password = TestPassword
        };

        // Act
        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/login",
                loginRequest);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var authentication =
            await response.Content
                .ReadFromJsonAsync<
                    AuthenticationResponse>();

        authentication.Should().NotBeNull();

        authentication!.UserId.Should().Be(
            registeredUser.UserId);

        authentication.RefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        authentication.Roles.Should().Contain(
            Roles.User);
    }

    [Fact]
    public async Task Deactivate_Should_Be_Idempotent()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: false);

        // Act
        using var secondResponse =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        var responseContent =
            await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_Should_Be_Idempotent()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                IsActive: true);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/active-status",
                request);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
    }

    private async Task SetActiveStatusAsAdminAsync(
        Guid userId,
        bool isActive)
    {
        AuthenticateAsAdmin();

        var request =
            new SetUserActiveStatusRequest(
                isActive);

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

    private async Task<AuthenticationResponse>
        RegisterUserAsync()
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var uniqueValue =
            Guid.NewGuid().ToString("N");

        var request = new RegisterRequest(
            FirstName: "Test",
            LastName: "Benutzer",
            Email:
                $"active-status-{uniqueValue}@tiermatch.test",
            Password: TestPassword);

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
                .ReadFromJsonAsync<
                    AuthenticationResponse>();

        authentication.Should().NotBeNull();

        authentication!.UserId.Should().NotBe(
            Guid.Empty);

        authentication.RefreshToken.Should()
            .NotBeNullOrWhiteSpace();

        return authentication;
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
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(
            new IdentityRole<Guid>(roleName));

        result.Succeeded.Should().BeTrue(
            string.Join(
                Environment.NewLine,
                result.Errors.Select(
                    error => error.Description)));
    }
}