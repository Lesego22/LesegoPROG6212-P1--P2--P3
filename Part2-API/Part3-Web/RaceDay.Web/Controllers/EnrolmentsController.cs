using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;


[SessionAuthorize(Roles.Participant)]
public class EnrolmentsController : AppController
{
    public EnrolmentsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetUserId();
        var result = await Api.GetAsync<MyEnrollmentList>($"api/enrollments/user/{userId}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;
        if (!result.Success) Flash(result.Error ?? "We could not load your enrolments.", false);

        return View(result.Data?.Enrollments ?? new List<MyEnrollment>());
    }
}
