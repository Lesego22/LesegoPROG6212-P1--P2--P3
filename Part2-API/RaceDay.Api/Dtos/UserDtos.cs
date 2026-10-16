using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public record UserProfileResponse(
    Guid UserId, string Email, string FirstName, string LastName,
    string? PhoneNumber, string Role, DateTime CreatedDate);

public class UpdateProfileRequest
{
    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}

public record UpdateProfileResponse(Guid UserId, string FirstName, string LastName, DateTime ModifiedDate);

