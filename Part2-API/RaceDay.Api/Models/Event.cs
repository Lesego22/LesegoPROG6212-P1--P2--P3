namespace RaceDay.Api.Models;

public class Event
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public string EventName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public int Capacity { get; set; }

    /// Run, Walk or Cycle (required by the Part 2 brief). Used the help of ChatGpt
    public string EventType { get; set; } = EventTypes.Run;

    /// Blob name of the banner image in Azure Blob Storage (for example banners/abc.jpg).
    public string? BannerImage { get; set; }

    public Guid OrganiserId { get; set; }
    public string Status { get; set; } = EventStatuses.Scheduled;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; } = DateTime.UtcNow;

    public AppUser Organiser { get; set; } = null!;
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
