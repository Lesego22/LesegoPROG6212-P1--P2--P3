using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IConfiguration _config;

    public AuthController(RaceDayDbContext db, ITokenService tokens, IConfiguration config)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
    }

    /// <summary>Register a new Organiser or Participant account.</summary>
    /// <remarks>
    /// Send the role as a roleId. Organiser = 11111111-1111-1111-1111-111111111111,
    /// Participant = 22222222-2222-2222-2222-222222222222.
    /// To stop anyone making themselves an Organiser, Organiser registration also needs the
    /// organiserAccessCode from appsettings.json. Returns a JWT so the user is logged in straight away.
    /// </remarks>
    /// <response code="201">Account created. Returns userId, email, token and role.</response>
    /// <response code="400">Invalid input (bad email, weak password, unknown role).</response>
    /// <response code="403">Organiser access code missing or wrong.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!request.Password.Any(char.IsLetter) || !request.Password.Any(char.IsDigit))
            return Error(400, "Password must contain at least one letter and one digit.");

        var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.UserRoleId == request.RoleId);
        if (role is null)
            return Error(400, "Unknown roleId. Use the Organiser or Participant role id.");

        if (role.RoleName == RoleNames.Organiser)
        {
            var expected = _config["Registration:OrganiserAccessCode"];
            if (string.IsNullOrWhiteSpace(expected) || request.OrganiserAccessCode != expected)
                return Error(403, "A valid organiserAccessCode is required to register as an Organiser.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Error(409, "An account with this email already exists.");

        var user = new AppUser
        {
            Email = email,
            // BCrypt hashes + salts the password. The plain password is never stored.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserRoleId = role.UserRoleId
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokens.CreateToken(user, role.RoleName);
        return Created($"/api/users/{user.UserId}", new AuthResponse(user.UserId, user.Email, token, role.RoleName));
    }

    /// <summary>Log in and receive a JWT token.</summary>
    /// <remarks>Copy the token, click "Authorize" in Swagger and paste it to call protected endpoints.</remarks>
    /// <response code="200">Login successful. Returns userId, email, token and role.</response>
    /// <response code="401">Email or password is wrong.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.UserRole).FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsActive || !PasswordMatches(request.Password, user.PasswordHash))
            return Error(401, "Invalid email or password.");

        var token = _tokens.CreateToken(user, user.UserRole.RoleName);
        return Ok(new AuthResponse(user.UserId, user.Email, token, user.UserRole.RoleName));
    }

    private static bool PasswordMatches(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch (Exception) { return false; } // a malformed hash must never crash login
    }
}
