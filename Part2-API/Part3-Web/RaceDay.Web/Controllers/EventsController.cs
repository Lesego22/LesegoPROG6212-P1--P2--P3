using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;


[SessionAuthorize(Roles.Participant)]
public class EventsController : AppController
{
    public EventsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(string? location, string? eventType, DateTime? from, DateTime? to)
    {
        
        var start = from?.Date ?? DateTime.Today;
        var query = new List<string>
        {
            "status=Scheduled",
            "dateFrom=" + Uri.EscapeDataString(start.ToString("yyyy-MM-ddTHH:mm:ss"))
        };
        if (to.HasValue)
            query.Add("dateTo=" + Uri.EscapeDataString(to.Value.Date.AddDays(1).AddSeconds(-1).ToString("yyyy-MM-ddTHH:mm:ss")));
        if (!string.IsNullOrWhiteSpace(location))
            query.Add("location=" + Uri.EscapeDataString(location.Trim()));
        if (!string.IsNullOrWhiteSpace(eventType))
            query.Add("eventType=" + Uri.EscapeDataString(eventType));

        var result = await Api.GetAsync<EventListResponse>("api/events?" + string.Join("&", query));
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;
        if (!result.Success) Flash(result.Error ?? "We could not load the events.", false);

        return View(new EventsIndexViewModel
        {
            Events = result.Data?.Events ?? new List<EventSummary>(),
            Location = location,
            EventType = eventType,
            From = from,
            To = to
        });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var result = await Api.GetAsync<EventDetail>($"api/events/{id}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success || result.Data is null)
        {
            Flash(result.Error ?? "We could not find that event.", false);
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(Guid eventId, Guid categoryId, string? emergencyContact)
    {
        var result = await Api.PostAsync<EnrollResult>("api/enrollments", new { eventId, categoryId, emergencyContact });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success)
        {
            Flash(result.Error ?? "We could not enrol you. Please try again.", false);
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        Flash("You are enrolled! The organiser will confirm your place.");
        return RedirectToAction("Index", "Enrolments");
    }
}
