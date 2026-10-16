using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[Authorize]
[Route("api")]
public class ResultsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    
    [HttpPost("results")]
    [Authorize(Roles = RoleNames.Organiser)]
    [ProducesResponseType(typeof(RecordResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecordResultResponse>> RecordResult([FromBody] RecordResultRequest request)
    {
        if (request.FinishTime <= TimeSpan.Zero || request.FinishTime >= TimeSpan.FromDays(1))
            return Error(400, "finishTime must be greater than 00:00:00 and less than 24 hours.");

        var enrolment = await _db.Enrolments
            .Include(en => en.EventCategory).ThenInclude(ec => ec.Event)
            .FirstOrDefaultAsync(en => en.EnrolmentId == request.EnrollmentId);
        if (enrolment is null) return Error(404, "Enrolment not found.");

        if (enrolment.EventCategory.Event.OrganiserId != CurrentUserId)
            return Error(403, "You can only record results for your own events.");

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolment.EnrolmentId))
            return Error(409, "A result has already been recorded for this enrolment.");

        var positionTaken = await _db.Results.AnyAsync(r =>
            r.Enrolment.EventCategoryId == enrolment.EventCategoryId && r.Position == request.Position);
        if (positionTaken)
            return Error(409, "That position is already taken in this category.");

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position,
            ChipNumber = request.ChipNumber?.Trim()
        };
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return Created($"/api/results/user/{enrolment.ParticipantId}",
            new RecordResultResponse(result.ResultId, result.Position, result.FinishTime, result.Status));
    }

    
    [HttpGet("results/user/{userId:guid}")]
    [ProducesResponseType(typeof(UserResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResultsResponse>> GetUserResults(Guid userId)
    {
        if (CurrentUserId != userId)
            return Error(403, "You can only view your own results.");

        var items = await _db.Results.AsNoTracking()
            .Where(r => r.Enrolment.ParticipantId == userId)
            .OrderByDescending(r => r.Enrolment.EventCategory.Event.EventDate)
            .Select(r => new UserResultDto(r.ResultId, r.Enrolment.EventCategory.Event.EventName,
                r.Enrolment.EventCategory.Category.CategoryName, r.Position, r.FinishTime,
                r.Enrolment.EventCategory.Event.EventDate))
            .ToListAsync();

        return Ok(new UserResultsResponse(items));
    }

    
    [HttpGet("events/{eventId:guid}/results")]
    [ProducesResponseType(typeof(EventResultsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResultsResponse>> GetEventResults(
        Guid eventId, [FromQuery] Guid? categoryId, [FromQuery] int? top)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return Error(404, "Event not found.");

        var query = _db.Results.AsNoTracking()
            .Where(r => r.Enrolment.EventCategory.EventId == eventId);
        if (categoryId.HasValue)
            query = query.Where(r => r.Enrolment.EventCategory.CategoryId == categoryId.Value);

        var ordered = query.OrderBy(r => r.Position).ThenBy(r => r.FinishTime);
        var limited = top.HasValue && top.Value > 0 ? ordered.Take(top.Value) : ordered;

        var items = await limited
            .Select(r => new EventResultDto(r.Position,
                r.Enrolment.Participant.FirstName + " " + r.Enrolment.Participant.LastName,
                r.FinishTime, r.ChipNumber, r.Enrolment.EventCategory.Category.CategoryName))
            .ToListAsync();

        return Ok(new EventResultsResponse(items));
    }
}
