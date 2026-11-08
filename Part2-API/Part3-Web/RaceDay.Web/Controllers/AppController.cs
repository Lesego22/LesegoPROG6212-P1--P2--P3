using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;


public abstract class AppController : Controller
{
    protected readonly ApiClient Api;

    protected AppController(ApiClient api) => Api = api;

   
    protected void Flash(string message, bool success = true) =>
        TempData[success ? "Success" : "Error"] = message;

    
    protected IActionResult? ExpiredSession(int statusCode)
    {
        if (statusCode != 401) return null;

        HttpContext.Session.Clear();
        Flash("Your session has ended. Please log in again.", false);
        return RedirectToAction("Login", "Account");
    }
}
