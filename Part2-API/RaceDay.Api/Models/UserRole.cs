namespace RaceDay.Api.Models
{
    public class UserRole
    {
        public Guid UserRoleId { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}

