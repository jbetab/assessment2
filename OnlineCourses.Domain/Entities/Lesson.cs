namespace OnlineCourses.Domain.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    
        public virtual Course Course { get; set; } = null!;
    }
}