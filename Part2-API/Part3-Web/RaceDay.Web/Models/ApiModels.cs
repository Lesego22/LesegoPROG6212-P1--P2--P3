namespace RaceDay.Web.Models;


public record AuthResponse(Guid UserId, string Email, string Token, string Role);

public record UserProfile(Guid UserId, string Email, string FirstName, string LastName,
    string? PhoneNumber, string Role, DateTime CreatedDate, string? ProfilePicture);

public record EventSummary(Guid EventId, string Name, DateTime Date, string Location,
    decimal Distance, int Capacity, string EventType, string Status, string? BannerImage);

public record EventListResponse(List<EventSummary> Events);

public record EventCategoryItem(Guid CategoryId, string Name);

public record EventDetail(Guid EventId, string Name, string? Description, DateTime Date, string Location,
    decimal Distance, int Capacity, string EventType, string Status, string? BannerImage,
    List<EventCategoryItem> Categories);

public record CreateEventResult(Guid EventId, string Name, string Status, DateTime CreatedDate);

public record OrganiserEvent(Guid EventId, string Name, DateTime Date, string Location,
    decimal Distance, int Capacity, string EventType, string Status, string? BannerImage, int EnrolmentCount);

public record OrganiserEventList(List<OrganiserEvent> Events);

public record CategoryItem(Guid CategoryId, string Name, string? Description, int? MinAge, int? MaxAge);

public record CategoryList(List<CategoryItem> Categories);

public record CreateCategoryResult(Guid CategoryId, string Name);

public record EnrollResult(Guid EnrollmentId, string Status, DateTime EnrollmentDate);

public record MyEnrollment(Guid EnrollmentId, Guid EventId, string EventName, DateTime EventDate,
    string CategoryName, string Status, DateTime EnrollmentDate);

public record MyEnrollmentList(List<MyEnrollment> Enrollments);

public record EventEnrollment(Guid EnrollmentId, string UserName, string Email,
    string CategoryName, string Status, DateTime EnrollmentDate);

public record EventEnrollmentList(List<EventEnrollment> Enrollments);

public record MyResult(Guid ResultId, string EventName, string CategoryName, int Position,
    TimeSpan FinishTime, DateTime EventDate, int TotalFinishers);

public record MyResultList(List<MyResult> Results);

public record EventResult(int Position, string UserName, TimeSpan FinishTime,
    string? ChipNumber, string CategoryName, Guid EnrollmentId);

public record EventResultList(List<EventResult> Results);

public record ImageUploadResponse(string ImageName);

