using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : AppController
{
    public AccountController(ApiClient api) : base(api) { }

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.IsLoggedIn()) return RedirectToAction("Index", "Home");
        return View(new LoginInput());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if (!ModelState.IsValid) return View(input);

        var result = await Api.PostAsync<AuthResponse>("api/auth/login", new { input.Email, input.Password });
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not log you in. Please try again.");
            return View(input);
        }

        await StartSession(result.Data);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsLoggedIn()) return RedirectToAction("Index", "Home");
        return View(new RegisterInput());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if (input.Role == Roles.Organiser && string.IsNullOrWhiteSpace(input.OrganiserCode))
            ModelState.AddModelError(nameof(input.OrganiserCode), "Organisers need an access code.");

        if (!ModelState.IsValid) return View(input);

        var roleId = input.Role == Roles.Organiser ? Roles.OrganiserId : Roles.ParticipantId;
        var result = await Api.PostAsync<AuthResponse>("api/auth/register", new
        {
            input.FirstName,
            input.LastName,
            input.Email,
            input.Password,
            RoleId = roleId,
            OrganiserAccessCode = input.OrganiserCode
        });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not create your account. Please try again.");
            return View(input);
        }

        await StartSession(result.Data);
        Flash("Welcome to RaceDay! Your account is ready.");
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        Flash("You have been logged out.");
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    
    private async Task StartSession(AuthResponse auth)
    {
        HttpContext.Session.SignIn(auth, auth.Email);

        var profile = await Api.GetAsync<UserProfile>($"api/users/{auth.UserId}");
        if (profile.Success && profile.Data is not null)
            HttpContext.Session.SetDisplayName(profile.Data.FirstName);
    }
}
