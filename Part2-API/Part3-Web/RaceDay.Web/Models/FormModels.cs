using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;


public class LoginInput
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterInput
{
    [Required(ErrorMessage = "Please enter your first name.")]
    [StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your last name.")]
    [StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a password.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Your password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please repeat your password.")]
    [Compare(nameof(Password), ErrorMessage = "The two passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = Roles.Participant;

    [Display(Name = "Organiser access code")]
    public string? OrganiserCode { get; set; }
}

public class EventForm
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Please enter the event name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the date and time.")]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(30).AddHours(7);

    [Required(ErrorMessage = "Please enter the location.")]
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 999.99, ErrorMessage = "Distance must be between 0.01 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal Distance { get; set; } = 10;

    [Range(1, 100000, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; } = 100;

    [Required]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = "Run";

    public string Status { get; set; } = "Scheduled";

    [Display(Name = "Banner image")]
    public IFormFile? Banner { get; set; }

    public string? BannerImage { get; set; }
}

public class CategoryForm
{
    [Required(ErrorMessage = "Please enter a category name.")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    [Display(Name = "Minimum age")]
    public int? MinAge { get; set; }

    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    [Display(Name = "Maximum age")]
    public int? MaxAge { get; set; }
}

public class ProfileForm
{
    [Required(ErrorMessage = "Please enter your first name.")]
    [StringLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your last name.")]
    [StringLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
}

public class ResultForm
{
    public Guid EnrollmentId { get; set; }

    [Required(ErrorMessage = "Please enter the finish time as hh:mm:ss.")]
    [Display(Name = "Finish time")]
    public TimeSpan? FinishTime { get; set; }

    [Range(1, 1000000, ErrorMessage = "Position must be 1 or higher.")]
    public int Position { get; set; }

    [StringLength(20)]
    [Display(Name = "Chip number")]
    public string? ChipNumber { get; set; }
}

