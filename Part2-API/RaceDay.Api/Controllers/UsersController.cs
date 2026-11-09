using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db) => _db = db;

    
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(Guid userId)
    {
        if (CurrentUserId != userId)
            return Error(403, "You can only view your own profile.");

        var user = await _db.Users.AsNoTracking().Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return Error(404, "User not found.");

        return Ok(new UserProfileResponse(user.UserId, user.Email, user.FirstName, user.LastName,
            user.PhoneNumber, user.UserRole.RoleName, user.CreatedDate, user.ProfilePicture));
    }

    
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile(Guid userId, [FromBody] UpdateProfileRequest request)
    {
        if (CurrentUserId != userId)
            return Error(403, "You can only update your own profile.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return Error(404, "User not found.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UpdateProfileResponse(user.UserId, user.FirstName, user.LastName, user.ModifiedDate.Value));
    }

    
    [HttpPost("{userId:guid}/picture")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageUploadResponse>> UploadPicture(
        Guid userId, IFormFile file, [FromServices] IBlobStorageService blobs)
    {
        if (CurrentUserId != userId)
            return Error(403, "You can only change your own profile picture.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return Error(404, "User not found.");

        var problem = ImageRules.Validate(file, out var extension);
        if (problem is not null) return Error(400, problem);

        try
        {
            await using var stream = file.OpenReadStream();
            user.ProfilePicture = await blobs.UploadAsync(stream, file.ContentType, "profiles", extension);
        }
        catch (Exception)
        {
            return Error(503, "Image storage is not available right now. Please try again.");
        }

        user.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new ImageUploadResponse(user.ProfilePicture!));
    }
}

