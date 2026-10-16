namespace RaceDay.Api.Models;

public class Result
{
    public Guid ResultId { get; set; } = Guid.NewGuid();
    public Guid EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string? ChipNumber { get; set; }
    public string Status { get; set; } = "Finished";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Enrolment Enrolment { get; set; } = null!;
}
