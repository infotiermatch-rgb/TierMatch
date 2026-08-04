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
public sealed class CurrentUserEndpointTests
    : IntegrationTestBase
{
    private const string TestPassword =
        "TierMatch-Test123!";

    public CurrentUserEndpointTests(
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
    public async Task GetCurrentUser_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Current_User_Data()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Paul",
            lastName: "Dittrich",
            emailPrefix: "current-user");

        AuthenticateAsUser(
            registeredUser.UserId);

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var currentUser =
            await response.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        currentUser.Should().NotBeNull();

        currentUser!.UserId.Should().Be(
            registeredUser.UserId);

        currentUser.Email.Should().Be(
            registeredUser.Email);

        currentUser.FirstName.Should().Be("Paul");
        currentUser.LastName.Should().Be("Dittrich");

        currentUser.Roles.Should().Contain(
            Roles.User);

        currentUser.ShelterId.Should().BeNull();
        currentUser.IsActive.Should().BeTrue();
        currentUser.CreatedAt.Should().NotBe(default);
        currentUser.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUser_Should_Load_Current_Roles_And_ShelterId_From_Database()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Saskia",
            lastName: "Tierheim",
            emailPrefix: "shelter-admin-me");

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        /*
         * Der Testclient erhält absichtlich nur die User-Rolle.
         * Der Endpunkt muss die aktuellen Rollen aus der
         * Datenbank laden und nicht aus den Test-Claims übernehmen.
         */
        AuthenticateAsUser(
            registeredUser.UserId);

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var currentUser =
            await response.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        currentUser.Should().NotBeNull();

        currentUser!.UserId.Should().Be(
            registeredUser.UserId);

        currentUser.ShelterId.Should().Be(
            shelterId);

        currentUser.Roles.Should().Contain(
            Roles.User);

        currentUser.Roles.Should().Contain(
            Roles.ShelterAdmin);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Forbidden_When_User_Is_Deactivated()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Gesperrter",
            lastName: "Benutzer",
            emailPrefix: "inactive-me");

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        AuthenticateAsUser(
            registeredUser.UserId);

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Unauthorized_When_User_Does_Not_Exist()
    {
        // Arrange
        AuthenticateAsUser(
            Guid.NewGuid());

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Contain_Last_Login_Time_After_Login()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Login",
            lastName: "Benutzer",
            emailPrefix: "last-login");

        await LoginUserAsync(
            registeredUser.Email);

        AuthenticateAsUser(
            registeredUser.UserId);

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/auth/me");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var currentUser =
            await response.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        currentUser.Should().NotBeNull();

        currentUser!.LastLoginAt.Should().NotBeNull();

        currentUser.LastLoginAt.Should().BeCloseTo(
            DateTime.UtcNow,
            TimeSpan.FromMinutes(1));
    }

    private async Task AssignShelterAccessAsAdminAsync(
        Guid userId,
        Guid shelterId)
    {
        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(
                shelterId);

        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/admin/users/{userId}/shelter-access",
            request);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
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
        RegisterUserAsync(
            string firstName,
            string lastName,
            string emailPrefix)
    {
        using var anonymousClient =
            CreateAnonymousClient();

        var uniqueValue =
            Guid.NewGuid().ToString("N");

        var request = new RegisterRequest(
            FirstName: firstName,
            LastName: lastName,
            Email:
                $"{emailPrefix}-{uniqueValue}@tiermatch.test",
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
                .ReadFromJsonAsync<AuthenticationResponse>();

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

        var request = new LoginRequest(
            Email: email,
            Password: TestPassword);

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
                .ReadFromJsonAsync<AuthenticationResponse>();

        authentication.Should().NotBeNull();

        return authentication!;
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