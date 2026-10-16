using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public static class TestHelpers
{
    
    public const string OrganiserCode = "RACEDAY-ORG-2026";
    public const string Password = "Passw0rd123";

    public static RegisterRequest NewRegisterRequest(bool organiser) => new()
    {
        Email = $"{Guid.NewGuid():N}@test.com",
        Password = Password,
        FirstName = "Test",
        LastName = organiser ? "Organiser" : "Participant",
        RoleId = organiser ? RoleIds.Organiser : RoleIds.Participant,
        OrganiserAccessCode = organiser ? OrganiserCode : null
    };

    
    public static async Task<AuthResponse> RegisterAsync(this RaceDayFactory factory, bool organiser)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", NewRegisterRequest(organiser));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    
    public static HttpClient ClientFor(this RaceDayFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    
    public static async Task<(Guid EventId, Guid CategoryId)> CreateEventWithCategoryAsync(
        HttpClient organiser, int capacity = 100)
    {
        var catResponse = await organiser.PostAsJsonAsync("/api/categories", new CreateCategoryRequest
        {
            Name = $"10km-{Guid.NewGuid():N}",
            Description = "Distance category"
        });
        catResponse.EnsureSuccessStatusCode();
        var category = (await catResponse.Content.ReadFromJsonAsync<CreateCategoryResponse>())!;

        var eventResponse = await organiser.PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Test Run",
            Description = "A test event",
            Date = DateTime.UtcNow.AddDays(30),
            Location = "Pretoria",
            Distance = 10m,
            Capacity = capacity,
            EventType = "Run",
            CategoryIds = new List<Guid> { category.CategoryId }
        });
        eventResponse.EnsureSuccessStatusCode();
        var created = (await eventResponse.Content.ReadFromJsonAsync<CreateEventResponse>())!;

        return (created.EventId, category.CategoryId);
    }

   
    public static async Task<Guid> EnrolAsync(HttpClient participant, Guid eventId, Guid categoryId)
    {
        var response = await participant.PostAsJsonAsync("/api/enrollments", new EnrollRequest
        {
            EventId = eventId,
            CategoryId = categoryId,
            EmergencyContact = "076 519 7864"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EnrollResponse>())!.EnrollmentId;
    }
}
