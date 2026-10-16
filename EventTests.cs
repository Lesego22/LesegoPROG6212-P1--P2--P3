using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;


public class EventTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EventTests(RaceDayFactory factory) => _factory = factory;

    private static CreateEventRequest ValidEvent() => new()
    {
        Name = "Pretoria 10K",
        Description = "Annual run",
        Date = DateTime.UtcNow.AddDays(30),
        Location = "Pretoria",
        Distance = 10m,
        Capacity = 500,
        EventType = "Run"
    };

    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);

        var response = await _factory.ClientFor(organiser.Token).PostAsJsonAsync("/api/events", ValidEvent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateEventResponse>();
        Assert.Equal("Scheduled", body!.Status);
    }

    [Fact]
    public async Task Organiser_CreateEvent_WithPastDate_ReturnsBadRequest()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var request = ValidEvent();
        request.Date = DateTime.UtcNow.AddDays(-1);

        var response = await _factory.ClientFor(organiser.Token).PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CreateEvent_WithInvalidEventType_ReturnsBadRequest()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var request = ValidEvent();
        request.EventType = "Swim";

        var response = await _factory.ClientFor(organiser.Token).PostAsJsonAsync("/api/events", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var client = _factory.ClientFor(organiser.Token);
        var created = await (await client.PostAsJsonAsync("/api/events", ValidEvent()))
            .Content.ReadFromJsonAsync<CreateEventResponse>();

        var update = new UpdateEventRequest
        {
            Name = "Renamed 10K",
            Description = "Updated",
            Date = DateTime.UtcNow.AddDays(40),
            Location = "Centurion",
            Distance = 10m,
            Capacity = 300,
            EventType = "Walk",
            Status = "Scheduled"
        };
        var response = await client.PutAsJsonAsync($"/api/events/{created!.EventId}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await client.GetFromJsonAsync<EventDetailResponse>($"/api/events/{created.EventId}");
        Assert.Equal("Renamed 10K", detail!.Name);
        Assert.Equal("Walk", detail.EventType);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var client = _factory.ClientFor(organiser.Token);
        var created = await (await client.PostAsJsonAsync("/api/events", ValidEvent()))
            .Content.ReadFromJsonAsync<CreateEventResponse>();

        var delete = await client.DeleteAsync($"/api/events/{created!.EventId}");
        var get = await client.GetAsync($"/api/events/{created.EventId}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent()
    {
        var owner = await _factory.RegisterAsync(organiser: true);
        var other = await _factory.RegisterAsync(organiser: true);
        var created = await (await _factory.ClientFor(owner.Token).PostAsJsonAsync("/api/events", ValidEvent()))
            .Content.ReadFromJsonAsync<CreateEventResponse>();

        var update = new UpdateEventRequest
        {
            Name = "Hijacked",
            Date = DateTime.UtcNow.AddDays(40),
            Location = "X",
            Distance = 5m,
            Capacity = 10,
            EventType = "Run",
            Status = "Scheduled"
        };
        var response = await _factory.ClientFor(other.Token).PutAsJsonAsync($"/api/events/{created!.EventId}", update);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanViewEventsAndCategories()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, _) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token));
        var participant = await _factory.RegisterAsync(organiser: false);
        var client = _factory.ClientFor(participant.Token);

        var list = await client.GetFromJsonAsync<EventListResponse>("/api/events");
        var detail = await client.GetFromJsonAsync<EventDetailResponse>($"/api/events/{eventId}");
        var categories = await client.GetFromJsonAsync<CategoryListResponse>("/api/categories");

        Assert.Contains(list!.Events, e => e.EventId == eventId);
        Assert.Single(detail!.Categories);
        Assert.NotEmpty(categories!.Categories);
    }

    [Fact]
    public async Task ListEvents_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_UnknownId_ReturnsNotFound()
    {
        var user = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(user.Token).GetAsync($"/api/events/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
