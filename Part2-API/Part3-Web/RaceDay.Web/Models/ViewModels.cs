namespace RaceDay.Web.Models;

public class EventsIndexViewModel
{
    public List<EventSummary> Events { get; set; } = new();
    public string? Location { get; set; }
    public string? EventType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class ManageCategoriesViewModel
{
    public EventDetail Event { get; set; } = null!;
    public List<CategoryItem> Available { get; set; } = new();
    public CategoryForm NewCategory { get; set; } = new();
}

public class ManageEnrolmentsViewModel
{
    public EventDetail Event { get; set; } = null!;
    public List<EventEnrollment> Enrollments { get; set; } = new();
}

public class ManageResultsViewModel
{
    public EventDetail Event { get; set; } = null!;
    public List<EventEnrollment> Awaiting { get; set; } = new();
    public List<EventResult> Results { get; set; } = new();
}
