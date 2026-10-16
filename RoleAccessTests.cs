using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;


public class RoleAccessTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public RoleAccessTests(RaceDayFactory factory) => _factory = factory;

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).PostAsJsonAsync("/api/events", new CreateEventRequest
        {
            Name = "Nope",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "X",
            Distance = 5m,
            Capacity = 10,
            EventType = "Run"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateCategory()
    {
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token)
            .PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Sneaky" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotDeleteEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, _) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token));
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).DeleteAsync($"/api/events/{eventId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewEventEnrolments()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, _) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token));
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).GetAsync($"/api/events/{eventId}/enrollments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotRecordResults()
    {
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).PostAsJsonAsync("/api/results", new RecordResultRequest
        {
            EnrollmentId = Guid.NewGuid(),
            FinishTime = TimeSpan.FromMinutes(45),
            Position = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrolInEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var client = _factory.ClientFor(organiser.Token);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(client);

        var response = await client.PostAsJsonAsync("/api/enrollments",
            new EnrollRequest { EventId = eventId, CategoryId = categoryId });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_IsRejectedFromProtectedEndpoints()
    {
        var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync("/api/events", new CreateEventRequest());
        var enrol = await client.PostAsJsonAsync("/api/enrollments", new EnrollRequest());
        var results = await client.GetAsync($"/api/events/{Guid.NewGuid()}/results");

        Assert.Equal(HttpStatusCode.Unauthorized, create.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, enrol.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, results.StatusCode);
    }

    [Fact]
    public async Task InvalidToken_IsRejected()
    {
        var client = _factory.ClientFor("this.is.not-a-real-token");

        var response = await client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
