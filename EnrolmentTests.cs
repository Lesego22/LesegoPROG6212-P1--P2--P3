using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;


public class EnrolmentTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EnrolmentTests(RaceDayFactory factory) => _factory = factory;

    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token));
        var participant = await _factory.RegisterAsync(organiser: false);
        var client = _factory.ClientFor(participant.Token);

        var response = await client.PostAsJsonAsync("/api/enrollments",
            new EnrollRequest { EventId = eventId, CategoryId = categoryId, EmergencyContact = "082 111 2222" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var enrol = await response.Content.ReadFromJsonAsync<EnrollResponse>();
        Assert.Equal("Active", enrol!.Status);

        // The link between participant, event and category must be stored
        var mine = await client.GetFromJsonAsync<UserEnrollmentsResponse>($"/api/enrollments/user/{participant.UserId}");
        var item = Assert.Single(mine!.Enrollments);
        Assert.Equal(enrol.EnrollmentId, item.EnrollmentId);
        Assert.Equal("Test Run", item.EventName);
    }

    [Fact]
    public async Task Participant_EnrollingTwiceInSameEvent_ReturnsConflict()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token));
        var participant = await _factory.RegisterAsync(organiser: false);
        var client = _factory.ClientFor(participant.Token);
        var request = new EnrollRequest { EventId = eventId, CategoryId = categoryId };

        var first = await client.PostAsJsonAsync("/api/enrollments", request);
        var second = await client.PostAsJsonAsync("/api/enrollments", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryNotOfferedAtEvent_ReturnsBadRequest()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var orgClient = _factory.ClientFor(organiser.Token);
        var (eventId, _) = await TestHelpers.CreateEventWithCategoryAsync(orgClient);
        var (_, otherCategoryId) = await TestHelpers.CreateEventWithCategoryAsync(orgClient); // category on a different event
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).PostAsJsonAsync("/api/enrollments",
            new EnrollRequest { EventId = eventId, CategoryId = otherCategoryId });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_InUnknownEvent_ReturnsNotFound()
    {
        var participant = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(participant.Token).PostAsJsonAsync("/api/enrollments",
            new EnrollRequest { EventId = Guid.NewGuid(), CategoryId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WhenEventIsFull_ReturnsConflict()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(organiser.Token), capacity: 1);
        var first = await _factory.RegisterAsync(organiser: false);
        var second = await _factory.RegisterAsync(organiser: false);

        await TestHelpers.EnrolAsync(_factory.ClientFor(first.Token), eventId, categoryId);
        var response = await _factory.ClientFor(second.Token).PostAsJsonAsync("/api/enrollments",
            new EnrollRequest { EventId = eventId, CategoryId = categoryId });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanViewEnrolmentsForOwnEvent()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var orgClient = _factory.ClientFor(organiser.Token);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(orgClient);
        var participant = await _factory.RegisterAsync(organiser: false);
        var enrolmentId = await TestHelpers.EnrolAsync(_factory.ClientFor(participant.Token), eventId, categoryId);

        var list = await orgClient.GetFromJsonAsync<EventEnrollmentsResponse>($"/api/events/{eventId}/enrollments");

        Assert.Contains(list!.Enrollments, e => e.EnrollmentId == enrolmentId);
    }

    [Fact]
    public async Task Organiser_CannotViewEnrolmentsForSomeoneElsesEvent()
    {
        var owner = await _factory.RegisterAsync(organiser: true);
        var other = await _factory.RegisterAsync(organiser: true);
        var (eventId, _) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(owner.Token));

        var response = await _factory.ClientFor(other.Token).GetAsync($"/api/events/{eventId}/enrollments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewAnotherParticipantsEnrolments()
    {
        var a = await _factory.RegisterAsync(organiser: false);
        var b = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(a.Token).GetAsync($"/api/enrollments/user/{b.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
