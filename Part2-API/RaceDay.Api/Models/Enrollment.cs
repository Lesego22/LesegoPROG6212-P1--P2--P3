namespace RaceDay.Api.Models;

/// Links a Participant to an Event + Category.
public class Enrolment
{
    public Guid EnrolmentId { get; set; } = Guid.NewGuid();
    public Guid ParticipantId { get; set; }
    public Guid EventCategoryId { get; set; }
    public string? EmergencyContact { get; set; }
    public string EnrolmentStatus { get; set; } = EnrolmentStatuses.Pending;
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; } = DateTime.UtcNow;

    public AppUser Participant { get; set; } = null!;
    public EventCategory EventCategory { get; set; } = null!;
    public Result? Result { get; set; }
}
