using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;


[SessionAuthorize(Roles.Participant)]
public class ResultsController : AppController
{
    public ResultsController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetUserId();
        var result = await Api.GetAsync<MyResultList>($"api/results/user/{userId}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;
        if (!result.Success) Flash(result.Error ?? "We could not load your results.", false);

        return View(result.Data?.Results ?? new List<MyResult>());
    }
}
