using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;


public class AuthTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public AuthTests(RaceDayFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_Participant_ReturnsCreatedWithTokenAndRole()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/auth/register", TestHelpers.NewRegisterRequest(organiser: false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(RoleNames.Participant, body.Role);
    }

    [Fact]
    public async Task Register_OrganiserWithCorrectCode_ReturnsCreated()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/auth/register", TestHelpers.NewRegisterRequest(organiser: true));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal(RoleNames.Organiser, body!.Role);
    }

    [Fact]
    public async Task Register_OrganiserWithoutAccessCode_ReturnsForbidden()
    {
        var request = TestHelpers.NewRegisterRequest(organiser: true);
        request.OrganiserAccessCode = null;

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var request = TestHelpers.NewRegisterRequest(organiser: false);

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        var second = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        var request = TestHelpers.NewRegisterRequest(organiser: false);
        request.Password = "short";

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UnknownRole_ReturnsBadRequest()
    {
        var request = TestHelpers.NewRegisterRequest(organiser: false);
        request.RoleId = Guid.NewGuid();

        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotThePlainPassword()
    {
        var request = TestHelpers.NewRegisterRequest(organiser: false);
        await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", request);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == request.Email);

        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash); // BCrypt hashes start with $2
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();
        var request = TestHelpers.NewRegisterRequest(organiser: false);
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = request.Email, Password = request.Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(RoleNames.Participant, body.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var request = TestHelpers.NewRegisterRequest(organiser: false);
        await client.PostAsJsonAsync("/api/auth/register", request);

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = request.Email, Password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Email = "nobody@test.com", Password = "Passw0rd123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_OwnProfile_CanBeViewedAndUpdated()
    {
        var user = await _factory.RegisterAsync(organiser: false);
        var client = _factory.ClientFor(user.Token);

        var get = await client.GetAsync($"/api/users/{user.UserId}");
        var put = await client.PutAsJsonAsync($"/api/users/{user.UserId}",
            new UpdateProfileRequest { FirstName = "Updated", LastName = "Name", PhoneNumber = "0821234567" });

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var updated = await put.Content.ReadFromJsonAsync<UpdateProfileResponse>();
        Assert.Equal("Updated", updated!.FirstName);
    }

    [Fact]
    public async Task Profile_AnotherUsersProfile_ReturnsForbidden()
    {
        var userA = await _factory.RegisterAsync(organiser: false);
        var userB = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(userA.Token).GetAsync($"/api/users/{userB.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/users/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
