using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize]
public class ProfileController : AppController
{
    public ProfileController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index()
    {
        var userId = HttpContext.Session.GetUserId();
        var result = await Api.GetAsync<UserProfile>($"api/users/{userId}");
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (!result.Success || result.Data is null)
        {
            Flash(result.Error ?? "We could not load your profile.", false);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Profile = result.Data;
        return View(new ProfileForm
        {
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            PhoneNumber = result.Data.PhoneNumber
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileForm form)
    {
        if (!ModelState.IsValid)
        {
            var profile = await Api.GetAsync<UserProfile>($"api/users/{HttpContext.Session.GetUserId()}");
            ViewBag.Profile = profile.Data;
            return View("Index", form);
        }

        var userId = HttpContext.Session.GetUserId();
        var result = await Api.PutAsync<object>($"api/users/{userId}",
            new { form.FirstName, form.LastName, form.PhoneNumber });
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success)
        {
            HttpContext.Session.SetDisplayName(form.FirstName);
            Flash("Your profile has been updated.");
        }
        else
        {
            Flash(result.Error ?? "We could not save your changes.", false);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Picture(IFormFile? picture)
    {
        if (picture is null || picture.Length == 0)
        {
            Flash("Please choose a picture first.", false);
            return RedirectToAction(nameof(Index));
        }

        var userId = HttpContext.Session.GetUserId();
        var result = await Api.UploadImageAsync($"api/users/{userId}/picture", picture);
        if (ExpiredSession(result.StatusCode) is { } expired) return expired;

        if (result.Success) Flash("Your profile picture has been updated.");
        else Flash(result.Error ?? "We could not upload that picture.", false);

        return RedirectToAction(nameof(Index));
    }
}
