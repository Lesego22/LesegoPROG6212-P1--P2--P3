using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class EnrollRequest
{
    public Guid EventId { get; set; }
    public Guid CategoryId { get; set; }

    [StringLength(100)]
    public string? EmergencyContact { get; set; }
}

public record EnrollResponse(Guid EnrollmentId, string Status, DateTime EnrollmentDate);

public record UserEnrollmentDto(Guid EnrollmentId, string EventName, string CategoryName, string Status, DateTime EnrollmentDate);

public record UserEnrollmentsResponse(List<UserEnrollmentDto> Enrollments);

public record EventEnrollmentDto(Guid EnrollmentId, string UserName, string Email, string CategoryName, string Status, DateTime EnrollmentDate);

public record EventEnrollmentsResponse(List<EventEnrollmentDto> Enrollments);
