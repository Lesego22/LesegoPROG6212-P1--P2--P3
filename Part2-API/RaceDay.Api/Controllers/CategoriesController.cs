using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Authorize]
[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(CategoryListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoryListResponse>> ListCategories()
    {
        var items = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryDto(c.CategoryId, c.CategoryName, c.Description, c.MinAge, c.MaxAge))
            .ToListAsync();
        return Ok(new CategoryListResponse(items));
    }

    
    [HttpPost]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CreateCategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateCategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (request.MinAge.HasValue && request.MaxAge.HasValue && request.MinAge > request.MaxAge)
            return Error(400, "minAge cannot be greater than maxAge.");

        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.CategoryName == name))
            return Error(409, "A category with this name already exists.");

        var category = new Category
        {
            CategoryName = name,
            Description = request.Description?.Trim(),
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created($"/api/categories/{category.CategoryId}",
            new CreateCategoryResponse(category.CategoryId, category.CategoryName));
    }
}
