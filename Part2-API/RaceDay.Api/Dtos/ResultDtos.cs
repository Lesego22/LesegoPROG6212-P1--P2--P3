using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class RecordResultRequest
{
    public Guid EnrollmentId { get; set; }

    public TimeSpan FinishTime { get; set; }

    [Range(1, 1000000)]
    public int Position { get; set; }

    [StringLength(20)]
    public string? ChipNumber { get; set; }
}

public record RecordResultResponse(Guid ResultId, int Position, TimeSpan FinishTime, string Status);

public record UserResultDto(Guid ResultId, string EventName, string CategoryName, int Position, TimeSpan FinishTime, DateTime EventDate);

public record UserResultsResponse(List<UserResultDto> Results);

public record EventResultDto(int Position, string UserName, TimeSpan FinishTime, string? ChipNumber, string CategoryName);

public record EventResultsResponse(List<EventResultDto> Results);
