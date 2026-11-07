using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[Route("api/images")]
[AllowAnonymous]
public class ImagesController : ApiControllerBase
{
    private readonly IBlobStorageService _blobs;

    public ImagesController(IBlobStorageService blobs) => _blobs = blobs;

    
    [HttpGet]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage([FromQuery] string name)
    {
        var valid = !string.IsNullOrWhiteSpace(name)
            && (name.StartsWith("banners/") || name.StartsWith("profiles/"))
            && !name.Contains("..");
        if (!valid) return Error(400, "That is not a valid image name.");

        try
        {
            var image = await _blobs.DownloadAsync(name);
            if (image is null) return Error(404, "Image not found.");
            return File(image.Value.Content, image.Value.ContentType);
        }
        catch (Exception)
        {
            return Error(503, "Image storage is not available right now.");
        }
    }
}
