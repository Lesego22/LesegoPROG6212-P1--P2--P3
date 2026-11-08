using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;


public class ImagesController : AppController
{
    public ImagesController(ApiClient api) : base(api) { }

    [HttpGet]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> Get(string name)
    {
        var image = await Api.GetImageAsync(name);
        if (image is null) return NotFound();

        return File(image.Value.Bytes, image.Value.ContentType);
    }
}
