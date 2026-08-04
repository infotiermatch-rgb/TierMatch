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
public sealed class CurrentUserProfileUpdateTests
    : IntegrationTestBase
{
    private const string TestPassword =
        "TierMatch-Test123!";

    public CurrentUserProfileUpdateTests(
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
    public async Task UpdateProfile_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        AuthenticateAsAnonymous();

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Neuer",
                LastName: "Name");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_Should_Update_FirstName_And_LastName()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "successful-update");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Paul",
                LastName: "Dittrich");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

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

        currentUser.FirstName.Should().Be("Paul");
        currentUser.LastName.Should().Be("Dittrich");

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var storedUser =
            await userManager.FindByIdAsync(
                registeredUser.UserId.ToString());

        storedUser.Should().NotBeNull();
        storedUser!.FirstName.Should().Be("Paul");
        storedUser.LastName.Should().Be("Dittrich");
    }

    [Fact]
    public async Task UpdateProfile_Should_Trim_FirstName_And_LastName()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "trim-update");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "   Paul   ",
                LastName: "   Dittrich   ");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

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
        currentUser!.FirstName.Should().Be("Paul");
        currentUser.LastName.Should().Be("Dittrich");
    }

    [Theory]
    [InlineData("", "Mustermann")]
    [InlineData("   ", "Mustermann")]
    [InlineData("Max", "")]
    [InlineData("Max", "   ")]
    public async Task UpdateProfile_Should_Return_BadRequest_When_Name_Is_Empty(
        string firstName,
        string lastName)
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "empty-name");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: firstName,
                LastName: lastName);

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        await AssertStoredNamesAsync(
            registeredUser.UserId,
            expectedFirstName: "Alter",
            expectedLastName: "Name");
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_BadRequest_When_FirstName_Is_Too_Long()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "long-first-name");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: new string('A', 101),
                LastName: "Mustermann");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        await AssertStoredNamesAsync(
            registeredUser.UserId,
            expectedFirstName: "Alter",
            expectedLastName: "Name");
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_BadRequest_When_LastName_Is_Too_Long()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "long-last-name");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Max",
                LastName: new string('B', 101));

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);

        await AssertStoredNamesAsync(
            registeredUser.UserId,
            expectedFirstName: "Alter",
            expectedLastName: "Name");
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_Forbidden_When_User_Is_Deactivated()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Gesperrter",
            lastName: "Benutzer",
            emailPrefix: "inactive-profile");

        await SetActiveStatusAsAdminAsync(
            registeredUser.UserId,
            isActive: false);

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Neuer",
                LastName: "Name");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);

        await AssertStoredNamesAsync(
            registeredUser.UserId,
            expectedFirstName: "Gesperrter",
            expectedLastName: "Benutzer");
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_Unauthorized_When_User_Does_Not_Exist()
    {
        // Arrange
        AuthenticateAsUser(
            Guid.NewGuid());

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Neuer",
                LastName: "Name");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_Should_Not_Change_Protected_User_Properties()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Alter",
            lastName: "Name",
            emailPrefix: "protected-properties");

        var shelterId =
            await CreateTestShelterAsync();

        await AssignShelterAccessAsAdminAsync(
            registeredUser.UserId,
            shelterId);

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Neuer",
                LastName: "Benutzername");

        // Act
        using var response =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

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

        currentUser!.Email.Should().Be(
            registeredUser.Email);

        currentUser.ShelterId.Should().Be(
            shelterId);

        currentUser.IsActive.Should().BeTrue();

        currentUser.Roles.Should().Contain(
            Roles.User);

        currentUser.Roles.Should().Contain(
            Roles.ShelterAdmin);

        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var storedUser =
            await userManager.FindByIdAsync(
                registeredUser.UserId.ToString());

        storedUser.Should().NotBeNull();

        storedUser!.Email.Should().Be(
            registeredUser.Email);

        storedUser.ShelterId.Should().Be(
            shelterId);

        storedUser.IsActive.Should().BeTrue();

        var roles =
            await userManager.GetRolesAsync(
                storedUser);

        roles.Should().Contain(Roles.User);
        roles.Should().Contain(Roles.ShelterAdmin);
    }

    [Fact]
    public async Task GetCurrentUser_Should_Return_Updated_Profile_After_Update()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Vorher",
            lastName: "Alt",
            emailPrefix: "get-after-update");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Nachher",
                LastName: "Neu");

        using var updateResponse =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        updateResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);

        // Act
        using var getResponse =
            await Client.GetAsync(
                "/api/v1/auth/me");

        // Assert
        var responseContent =
            await getResponse.Content.ReadAsStringAsync();

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var currentUser =
            await getResponse.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        currentUser.Should().NotBeNull();
        currentUser!.FirstName.Should().Be("Nachher");
        currentUser.LastName.Should().Be("Neu");
    }

    [Fact]
    public async Task UpdateProfile_Should_Be_Idempotent()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Paul",
            lastName: "Dittrich",
            emailPrefix: "idempotent-profile");

        AuthenticateAsUser(
            registeredUser.UserId);

        var request =
            new UpdateCurrentUserProfileRequest(
                FirstName: "Paul",
                LastName: "Dittrich");

        // Act
        using var firstResponse =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        using var secondResponse =
            await Client.PatchAsJsonAsync(
                "/api/v1/auth/me",
                request);

        // Assert
        firstResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);

        secondResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);

        var currentUser =
            await secondResponse.Content
                .ReadFromJsonAsync<CurrentUserResponse>();

        currentUser.Should().NotBeNull();
        currentUser!.FirstName.Should().Be("Paul");
        currentUser.LastName.Should().Be("Dittrich");
    }

    private async Task AssignShelterAccessAsAdminAsync(
        Guid userId,
        Guid shelterId)
    {
        AuthenticateAsAdmin();

        var request =
            new AssignShelterAdminRequest(
                shelterId);

        using var response =
            await Client.PutAsJsonAsync(
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

    private async Task AssertStoredNamesAsync(
        Guid userId,
        string expectedFirstName,
        string expectedLastName)
    {
        using var scope =
            Factory.Services.CreateScope();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<
                    UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(
            userId.ToString());

        user.Should().NotBeNull();

        user!.FirstName.Should().Be(
            expectedFirstName);

        user.LastName.Should().Be(
            expectedLastName);
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