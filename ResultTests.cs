using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Dtos;

namespace RaceDay.Api.Tests;


public class ResultTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public ResultTests(RaceDayFactory factory) => _factory = factory;

    [Fact]
    public async Task Organiser_CanRecordResult_AndParticipantCanViewIt()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var orgClient = _factory.ClientFor(organiser.Token);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(orgClient);
        var participant = await _factory.RegisterAsync(organiser: false);
        var partClient = _factory.ClientFor(participant.Token);
        var enrolmentId = await TestHelpers.EnrolAsync(partClient, eventId, categoryId);

        var record = await orgClient.PostAsJsonAsync("/api/results", new RecordResultRequest
        {
            EnrollmentId = enrolmentId,
            FinishTime = new TimeSpan(0, 45, 32),
            Position = 1,
            ChipNumber = "CHIP001"
        });

        Assert.Equal(HttpStatusCode.Created, record.StatusCode);

        var mine = await partClient.GetFromJsonAsync<UserResultsResponse>($"/api/results/user/{participant.UserId}");
        var result = Assert.Single(mine!.Results);
        Assert.Equal(1, result.Position);
        Assert.Equal(new TimeSpan(0, 45, 32), result.FinishTime);

        var board = await partClient.GetFromJsonAsync<EventResultsResponse>($"/api/events/{eventId}/results");
        Assert.Single(board!.Results);
    }

    [Fact]
    public async Task RecordResult_Twice_ForSameEnrolment_ReturnsConflict()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);
        var orgClient = _factory.ClientFor(organiser.Token);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(orgClient);
        var participant = await _factory.RegisterAsync(organiser: false);
        var enrolmentId = await TestHelpers.EnrolAsync(_factory.ClientFor(participant.Token), eventId, categoryId);
        var request = new RecordResultRequest { EnrollmentId = enrolmentId, FinishTime = new TimeSpan(0, 50, 0), Position = 1 };

        var first = await orgClient.PostAsJsonAsync("/api/results", request);
        var second = await orgClient.PostAsJsonAsync("/api/results", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task RecordResult_ForUnknownEnrolment_ReturnsNotFound()
    {
        var organiser = await _factory.RegisterAsync(organiser: true);

        var response = await _factory.ClientFor(organiser.Token).PostAsJsonAsync("/api/results", new RecordResultRequest
        {
            EnrollmentId = Guid.NewGuid(),
            FinishTime = new TimeSpan(0, 45, 0),
            Position = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotRecordResultForSomeoneElsesEvent()
    {
        var owner = await _factory.RegisterAsync(organiser: true);
        var other = await _factory.RegisterAsync(organiser: true);
        var (eventId, categoryId) = await TestHelpers.CreateEventWithCategoryAsync(_factory.ClientFor(owner.Token));
        var participant = await _factory.RegisterAsync(organiser: false);
        var enrolmentId = await TestHelpers.EnrolAsync(_factory.ClientFor(participant.Token), eventId, categoryId);

        var response = await _factory.ClientFor(other.Token).PostAsJsonAsync("/api/results", new RecordResultRequest
        {
            EnrollmentId = enrolmentId,
            FinishTime = new TimeSpan(0, 45, 0),
            Position = 1
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewAnotherParticipantsResults()
    {
        var a = await _factory.RegisterAsync(organiser: false);
        var b = await _factory.RegisterAsync(organiser: false);

        var response = await _factory.ClientFor(a.Token).GetAsync($"/api/results/user/{b.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
