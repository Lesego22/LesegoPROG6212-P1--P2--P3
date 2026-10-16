using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    
    protected Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirst("sub")?.Value, out var id) ? (Guid?)id : null;

    
    protected ObjectResult Error(int statusCode, string detail) =>
        Problem(detail: detail, statusCode: statusCode);
}
