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
public sealed class AdminUserAuthorizationTests
    : IntegrationTestBase
{
    private const string TestPassword =
        "TierMatch-Test123!";

    public AdminUserAuthorizationTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await EnsureIdentityRolesAsync();
    }

    /*
     * ---------------------------------------------------------
     * Tierheimzugriff zuweisen
     * ---------------------------------------------------------
     */

    [Fact]
    public async Task AssignShelterAccess_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        AuthenticateAsAnonymous();

        var request =
            new AssignShelterAdminRequest(shelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access",
            request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AssignShelterAccess_Should_Return_Forbidden_When_User_Has_User_Role()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        AuthenticateAsUser();

        var request =
            new AssignShelterAdminRequest(shelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access",
            request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignShelterAccess_Should_Return_Forbidden_When_User_Is_ShelterAdmin()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        AuthenticateAsShelterAdmin(shelterId);

        var request =
            new AssignShelterAdminRequest(shelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access",
            request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Assign_ShelterAdmin_Role_And_ShelterId()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(shelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access",
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

        user!.ShelterId.Should().Be(shelterId);

        var isShelterAdmin =
            await userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        isShelterAdmin.Should().BeTrue();

        var isUser =
            await userManager.IsInRoleAsync(
                user,
                Roles.User);

        isUser.Should().BeTrue();
    }

    [Fact]
    public async Task Admin_Should_Return_NotFound_When_Assign_User_Does_Not_Exist()
    {
        // Arrange
        var missingUserId =
            Guid.NewGuid();

        var shelterId =
            await CreateTestShelterAsync();

        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(shelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{missingUserId}/shelter-access",
            request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Admin_Should_Return_NotFound_When_Assign_Shelter_Does_Not_Exist()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var missingShelterId =
            Guid.NewGuid();

        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(
                missingShelterId);

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access",
            request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.ShelterId.Should().BeNull();

        var isShelterAdmin =
            await userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        isShelterAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task AssignShelterAccess_Should_Revoke_Existing_Refresh_Token()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        using var anonymousClient =
            CreateAnonymousClient();

        var refreshRequest = new
        {
            refreshToken =
                registeredUser.RefreshToken
        };

        // Act
        using var refreshResponse =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/refresh",
                refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    /*
     * ---------------------------------------------------------
     * Tierheimzugriff entziehen
     * ---------------------------------------------------------
     */

    [Fact]
    public async Task RemoveShelterAccess_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveShelterAccess_Should_Return_Forbidden_When_User_Has_User_Role()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsUser();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveShelterAccess_Should_Return_Forbidden_When_User_Is_ShelterAdmin()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Return_NotFound_When_Remove_User_Does_Not_Exist()
    {
        // Arrange
        AuthenticateAsAdmin();

        var missingUserId =
            Guid.NewGuid();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/admin/users/{missingUserId}/shelter-access");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Admin_Should_Remove_ShelterAdmin_Role_And_ShelterId()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

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

        user!.ShelterId.Should().BeNull();

        var isShelterAdmin =
            await userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        isShelterAdmin.Should().BeFalse();

        /*
         * Die normale Benutzerrolle bleibt erhalten.
         */
        var isUser =
            await userManager.IsInRoleAsync(
                user,
                Roles.User);

        isUser.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveShelterAccess_Should_Revoke_Existing_Refresh_Token()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        /*
         * Die Registrierungssitzung wurde bereits durch die
         * Zuweisung widerrufen. Deshalb melden wir den Benutzer
         * erneut an und erhalten einen neuen aktiven Refresh Token.
         */
        var shelterAdminSession =
            await LoginUserAsync(
                registeredUser.Email);

        shelterAdminSession.Roles.Should().Contain(
            Roles.ShelterAdmin);

        AuthenticateAsAdmin();

        using var removeResponse =
            await Client.DeleteAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        var removeResponseContent =
            await removeResponse.Content.ReadAsStringAsync();

        removeResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {removeResponseContent}");

        using var anonymousClient =
            CreateAnonymousClient();

        var refreshRequest = new
        {
            refreshToken =
                shelterAdminSession.RefreshToken
        };

        // Act
        using var refreshResponse =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/refresh",
                refreshRequest);

        // Assert
        refreshResponse.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveShelterAccess_Should_Be_Idempotent()
    {
        // Arrange
        var registeredUser =
            await RegisterUserAsync();

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsAdmin();

        using var firstResponse =
            await Client.DeleteAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        firstResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        // Act
        using var secondResponse =
            await Client.DeleteAsync(
                $"/api/v1/admin/users/{registeredUser.UserId}/shelter-access");

        // Assert
        var secondResponseContent =
            await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {secondResponseContent}");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            registeredUser.UserId.ToString());

        user.Should().NotBeNull();
        user!.ShelterId.Should().BeNull();

        var isShelterAdmin =
            await userManager.IsInRoleAsync(
                user,
                Roles.ShelterAdmin);

        isShelterAdmin.Should().BeFalse();
    }

    /*
     * ---------------------------------------------------------
     * Hilfsmethoden
     * ---------------------------------------------------------
     */

    private async Task AssignShelterAccessAsAdminAsync(
        Guid userId,
        Guid shelterId)
    {
        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(shelterId);

        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{userId}/shelter-access",
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
                $"test-user-{uniqueValue}@tiermatch.test",
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

    private async Task<AuthenticationResponse>
        LoginUserAsync(
            string email)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var request = new
        {
            email,
            password = TestPassword
        };

        using var response =
            await anonymousClient.PostAsJsonAsync(
                "/api/v1/auth/login",
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

        authentication!.RefreshToken.Should()
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