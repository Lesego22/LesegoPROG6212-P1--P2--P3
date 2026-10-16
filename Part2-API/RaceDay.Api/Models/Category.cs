namespace RaceDay.Api.Models
{
    public class Category
    {
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    }

}

