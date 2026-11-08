using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Services;

public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string? _role;

    public SessionAuthorizeAttribute(string? role = null) => _role = role;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!session.IsLoggedIn())
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (_role is not null && session.GetRole() != _role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
