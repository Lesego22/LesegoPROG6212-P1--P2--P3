using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class EventRequestBase
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    [Required, StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 999.99)]
    public decimal Distance { get; set; }

    [Range(1, 1000000)]
    public int Capacity { get; set; }

    [Required]
    public string EventType { get; set; } = "Run";
}

public class CreateEventRequest : EventRequestBase
{
    public List<Guid> CategoryIds { get; set; } = new();
}

public class UpdateEventRequest : EventRequestBase
{
    [Required]
    public string Status { get; set; } = "Scheduled";
}

public class LinkCategoryRequest
{
    public Guid CategoryId { get; set; }
}

public record EventSummaryDto(
    Guid EventId, string Name, DateTime Date, string Location,
    decimal Distance, int Capacity, string EventType, string Status);

public record EventListResponse(List<EventSummaryDto> Events);

public record CreateEventResponse(Guid EventId, string Name, string Status, DateTime CreatedDate);

public record EventCategoryDto(Guid CategoryId, string Name);

public record EventDetailResponse(
    Guid EventId, string Name, string? Description, DateTime Date, string Location,
    decimal Distance, int Capacity, string EventType, string Status,
    List<EventCategoryDto> Categories);

public record UpdateEventResponse(Guid EventId, DateTime ModifiedDate);
