using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Authorize]
[Route("api/events")]
public class EventsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db) => _db = db;

    
    [HttpGet]
    [ProducesResponseType(typeof(EventListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EventListResponse>> ListEvents(
        [FromQuery] string? status, [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo, [FromQuery] string? location)
    {
        var query = _db.Events.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => e.Status == status);
        if (dateFrom.HasValue) query = query.Where(e => e.EventDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(e => e.EventDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(location)) query = query.Where(e => e.Location.Contains(location));

        var items = await query.OrderBy(e => e.EventDate)
            .Select(e => new EventSummaryDto(e.EventId, e.EventName, e.EventDate, e.Location,
                e.Distance, e.Capacity, e.EventType, e.Status))
            .ToListAsync();

        return Ok(new EventListResponse(items));
    }

    
    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(typeof(EventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailResponse>> GetEvent(Guid eventId)
    {
        var ev = await _db.Events.AsNoTracking()
            .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
            .FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return Error(404, "Event not found.");

        var categories = ev.EventCategories
            .Select(ec => new EventCategoryDto(ec.CategoryId, ec.Category.CategoryName)).ToList();

        return Ok(new EventDetailResponse(ev.EventId, ev.EventName, ev.Description, ev.EventDate,
            ev.Location, ev.Distance, ev.Capacity, ev.EventType, ev.Status, categories));
    }

   
    [HttpPost]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(CreateEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CreateEventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var organiserId = CurrentUserId;
        if (organiserId is null) return Error(401, "Invalid token.");

        if (request.Date <= DateTime.UtcNow)
            return Error(400, "The event date must be in the future.");
        if (!EventTypes.TryNormalise(request.EventType, out var eventType))
            return Error(400, "eventType must be Run, Walk or Cycle.");

        var categoryIds = request.CategoryIds.Distinct().ToList();
        if (categoryIds.Count > 0)
        {
            var found = await _db.Categories.CountAsync(c => categoryIds.Contains(c.CategoryId));
            if (found != categoryIds.Count)
                return Error(400, "One or more categoryIds do not exist.");
        }

        var ev = new Event
        {
            EventName = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.Date,
            Location = request.Location.Trim(),
            Distance = request.Distance,
            Capacity = request.Capacity,
            EventType = eventType,
            OrganiserId = organiserId.Value,
            Status = EventStatuses.Scheduled
        };
        foreach (var categoryId in categoryIds)
            ev.EventCategories.Add(new EventCategory { CategoryId = categoryId });

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return Created($"/api/events/{ev.EventId}",
            new CreateEventResponse(ev.EventId, ev.EventName, ev.Status, ev.CreatedDate));
    }

    
    [HttpPut("{eventId:guid}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(UpdateEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateEventResponse>> UpdateEvent(Guid eventId, [FromBody] UpdateEventRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return Error(404, "Event not found.");
        if (ev.OrganiserId != CurrentUserId) return Error(403, "You can only update your own events.");

        if (!EventTypes.TryNormalise(request.EventType, out var eventType))
            return Error(400, "eventType must be Run, Walk or Cycle.");
        if (!EventStatuses.TryNormalise(request.Status, out var status))
            return Error(400, "status must be Scheduled, Cancelled or Completed.");

        ev.EventName = request.Name.Trim();
        ev.Description = request.Description?.Trim();
        ev.EventDate = request.Date;
        ev.Location = request.Location.Trim();
        ev.Distance = request.Distance;
        ev.Capacity = request.Capacity;
        ev.EventType = eventType;
        ev.Status = status;
        ev.ModifiedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new UpdateEventResponse(ev.EventId, ev.ModifiedDate.Value));
    }

    
    [HttpDelete("{eventId:guid}")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteEvent(Guid eventId)
    {
        var ev = await _db.Events.Include(e => e.EventCategories)
            .FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return Error(404, "Event not found.");
        if (ev.OrganiserId != CurrentUserId) return Error(403, "You can only delete your own events.");

        var hasEnrolments = await _db.Enrolments.AnyAsync(en => en.EventCategory.EventId == eventId);
        if (hasEnrolments)
            return Error(409, "This event already has enrolments. Set its status to Cancelled instead of deleting it.");

        _db.EventCategories.RemoveRange(ev.EventCategories);
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpPost("{eventId:guid}/categories")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventCategoryDto>> AddCategoryToEvent(Guid eventId, [FromBody] LinkCategoryRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return Error(404, "Event not found.");
        if (ev.OrganiserId != CurrentUserId) return Error(403, "You can only change your own events.");

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);
        if (category is null) return Error(404, "Category not found.");

        if (await _db.EventCategories.AnyAsync(ec => ec.EventId == eventId && ec.CategoryId == request.CategoryId))
            return Error(409, "This category is already offered at the event.");

        _db.EventCategories.Add(new EventCategory { EventId = eventId, CategoryId = request.CategoryId });
        await _db.SaveChangesAsync();

        return Created($"/api/events/{eventId}", new EventCategoryDto(category.CategoryId, category.CategoryName));
    }
}

