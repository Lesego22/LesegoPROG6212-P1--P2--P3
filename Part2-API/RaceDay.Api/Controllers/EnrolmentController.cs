using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;


[Authorize]
[Route("api")]
public class EnrollmentsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrollmentsController(RaceDayDbContext db) => _db = db;

    
    [HttpPost("enrollments")]
    [Authorize(Roles = RoleNames.Participant)]
    [ProducesResponseType(typeof(EnrollResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EnrollResponse>> Enroll([FromBody] EnrollRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null) return Error(401, "Invalid token.");

        var eventCategory = await _db.EventCategories.Include(ec => ec.Event)
            .FirstOrDefaultAsync(ec => ec.EventId == request.EventId && ec.CategoryId == request.CategoryId);

        if (eventCategory is null)
        {
            var eventExists = await _db.Events.AnyAsync(e => e.EventId == request.EventId);
            return eventExists
                ? Error(400, "That category is not offered at this event.")
                : Error(404, "Event not found.");
        }

        var ev = eventCategory.Event;
        if (ev.Status != EventStatuses.Scheduled || ev.EventDate <= DateTime.UtcNow)
            return Error(400, "This event is not open for enrolment.");

        var alreadyEnrolled = await _db.Enrolments.AnyAsync(en =>
            en.ParticipantId == userId.Value &&
            en.EventCategory.EventId == request.EventId &&
            en.EnrolmentStatus == EnrolmentStatuses.Active);
        if (alreadyEnrolled)
            return Error(409, "You are already enrolled in this event.");

        var taken = await _db.Enrolments.CountAsync(en =>
            en.EventCategory.EventId == request.EventId &&
            en.EnrolmentStatus == EnrolmentStatuses.Active);
        if (taken >= ev.Capacity)
            return Error(409, "This event is full.");

        var enrolment = new Enrolment
        {
            ParticipantId = userId.Value,
            EventCategoryId = eventCategory.EventCategoryId,
            EmergencyContact = request.EmergencyContact?.Trim()
        };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Created($"/api/enrollments/user/{userId.Value}",
            new EnrollResponse(enrolment.EnrolmentId, enrolment.EnrolmentStatus, enrolment.EnrolmentDate));
    }

    
    [HttpGet("enrollments/user/{userId:guid}")]
    [ProducesResponseType(typeof(UserEnrollmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserEnrollmentsResponse>> GetUserEnrollments(Guid userId)
    {
        if (CurrentUserId != userId)
            return Error(403, "You can only view your own enrolments.");

        var items = await _db.Enrolments.AsNoTracking()
            .Where(en => en.ParticipantId == userId)
            .OrderByDescending(en => en.EnrolmentDate)
            .Select(en => new UserEnrollmentDto(en.EnrolmentId, en.EventCategory.Event.EventName,
                en.EventCategory.Category.CategoryName, en.EnrolmentStatus, en.EnrolmentDate))
            .ToListAsync();

        return Ok(new UserEnrollmentsResponse(items));
    }

    
    [HttpGet("events/{eventId:guid}/enrollments")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(EventEnrollmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventEnrollmentsResponse>> GetEventEnrollments(
        Guid eventId, [FromQuery] Guid? categoryId, [FromQuery] string? status)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return Error(404, "Event not found.");
        if (ev.OrganiserId != CurrentUserId) return Error(403, "You can only view enrolments for your own events.");

        var query = _db.Enrolments.AsNoTracking().Where(en => en.EventCategory.EventId == eventId);
        if (categoryId.HasValue) query = query.Where(en => en.EventCategory.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(en => en.EnrolmentStatus == status);

        var items = await query.OrderBy(en => en.EnrolmentDate)
            .Select(en => new EventEnrollmentDto(en.EnrolmentId,
                en.Participant.FirstName + " " + en.Participant.LastName, en.Participant.Email,
                en.EventCategory.Category.CategoryName, en.EnrolmentStatus, en.EnrolmentDate))
            .ToListAsync();

        return Ok(new EventEnrollmentsResponse(items));
    }
}

