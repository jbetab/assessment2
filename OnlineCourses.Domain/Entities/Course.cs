using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Domain.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public bool CanBePublished()
        {
            return Lessons.Any(l => !l.IsDeleted);
        }
    }
}