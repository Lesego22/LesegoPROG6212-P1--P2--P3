using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
   
    public IActionResult Index()
    {
        var session = HttpContext.Session;

        if (!session.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        return session.GetRole() == Roles.Organiser
            ? RedirectToAction("Dashboard", "Manage")
            : RedirectToAction("Index", "Events");
    }

    public IActionResult Error() => View();
}
