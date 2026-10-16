namespace RaceDay.Api.Models
{
    public class EventCategory
    {
        public Guid EventCategoryId { get; set; } = Guid.NewGuid();
        public Guid EventId { get; set; }
        public Guid CategoryId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Event Event { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }


}

