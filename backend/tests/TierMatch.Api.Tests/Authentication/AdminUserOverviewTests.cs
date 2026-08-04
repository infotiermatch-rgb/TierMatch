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
public sealed class AdminUserOverviewTests
    : IntegrationTestBase
{
    private const string TestPassword =
        "TierMatch-Test123!";

    public AdminUserOverviewTests(
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
    public async Task GetUsers_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_Should_Return_Forbidden_When_User_Has_User_Role()
    {
        // Arrange
        AuthenticateAsUser();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_Should_Return_Forbidden_When_User_Is_ShelterAdmin()
    {
        // Arrange
        AuthenticateAsShelterAdmin(
            Guid.NewGuid());

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Get_User_List()
    {
        // Arrange
        var firstUser = await RegisterUserAsync(
            firstName: "Anna",
            lastName: "Muster",
            emailPrefix: "anna");

        var secondUser = await RegisterUserAsync(
            firstName: "Bernd",
            lastName: "Beispiel",
            emailPrefix: "bernd");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users?page=1&pageSize=20");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var result =
            await response.Content
                .ReadFromJsonAsync<AdminUserListResponse>();

        result.Should().NotBeNull();

        result!.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items.Should().HaveCount(2);

        result.Items.Should().Contain(user =>
            user.Id == firstUser.UserId &&
            user.Email == firstUser.Email &&
            user.FirstName == "Anna" &&
            user.LastName == "Muster" &&
            user.IsActive &&
            user.ShelterId == null &&
            user.Roles.Contains(Roles.User));

        result.Items.Should().Contain(user =>
            user.Id == secondUser.UserId &&
            user.Email == secondUser.Email &&
            user.FirstName == "Bernd" &&
            user.LastName == "Beispiel" &&
            user.IsActive &&
            user.ShelterId == null &&
            user.Roles.Contains(Roles.User));
    }

    [Fact]
    public async Task Admin_Should_Search_Users_By_Name()
    {
        // Arrange
        var matchingUser = await RegisterUserAsync(
            firstName: "Paul",
            lastName: "Dittrich",
            emailPrefix: "paul");

        await RegisterUserAsync(
            firstName: "Max",
            lastName: "Mustermann",
            emailPrefix: "max");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users?search=paul&page=1&pageSize=20");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var result =
            await response.Content
                .ReadFromJsonAsync<AdminUserListResponse>();

        result.Should().NotBeNull();

        result!.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();

        var user = result.Items.Single();

        user.Id.Should().Be(
            matchingUser.UserId);

        user.FirstName.Should().Be("Paul");
        user.LastName.Should().Be("Dittrich");
    }

    [Fact]
    public async Task Admin_Should_Search_Users_By_Email_Case_Insensitively()
    {
        // Arrange
        var matchingUser = await RegisterUserAsync(
            firstName: "Lisa",
            lastName: "Schmidt",
            emailPrefix: "special-search");

        await RegisterUserAsync(
            firstName: "Tom",
            lastName: "Lehmann",
            emailPrefix: "other-user");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users?search=SPECIAL-SEARCH&page=1&pageSize=20");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var result =
            await response.Content
                .ReadFromJsonAsync<AdminUserListResponse>();

        result.Should().NotBeNull();

        result!.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();

        result.Items.Single().Id.Should().Be(
            matchingUser.UserId);
    }

    [Fact]
    public async Task Admin_Should_Get_Paginated_User_List()
    {
        // Arrange
        await RegisterUserAsync(
            firstName: "Anton",
            lastName: "Alpha",
            emailPrefix: "anton");

        var secondUser = await RegisterUserAsync(
            firstName: "Berta",
            lastName: "Beta",
            emailPrefix: "berta");

        await RegisterUserAsync(
            firstName: "Clara",
            lastName: "Gamma",
            emailPrefix: "clara");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/admin/users?page=2&pageSize=1");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var result =
            await response.Content
                .ReadFromJsonAsync<AdminUserListResponse>();

        result.Should().NotBeNull();

        result!.TotalCount.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.Items.Should().ContainSingle();

        result.Items.Single().Id.Should().Be(
            secondUser.UserId);
    }

    [Fact]
    public async Task Admin_Should_Get_User_By_Id()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Julia",
            lastName: "Test",
            emailPrefix: "julia");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var user =
            await response.Content
                .ReadFromJsonAsync<AdminUserDto>();

        user.Should().NotBeNull();

        user!.Id.Should().Be(
            registeredUser.UserId);

        user.Email.Should().Be(
            registeredUser.Email);

        user.FirstName.Should().Be("Julia");
        user.LastName.Should().Be("Test");
        user.IsActive.Should().BeTrue();
        user.ShelterId.Should().BeNull();
        user.Roles.Should().Contain(Roles.User);
        user.CreatedAt.Should().NotBe(default);
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public async Task GetUserById_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var registeredUser = await RegisterUserAsync(
            firstName: "Anonym",
            lastName: "Test",
            emailPrefix: "anonymous-detail");

        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.GetAsync(
            $"/api/v1/admin/users/{registeredUser.UserId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        AuthenticateAsAdmin();

        var missingUserId =
            Guid.NewGuid();

        // Act
        using var response = await Client.GetAsync(
            $"/api/v1/admin/users/{missingUserId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetUsers_Should_Return_BadRequest_For_Invalid_Pagination(
        int page,
        int pageSize)
    {
        // Arrange
        AuthenticateAsAdmin();

        // Act
        using var response = await Client.GetAsync(
            $"/api/v1/admin/users?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
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

        authentication.Email.Should()
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