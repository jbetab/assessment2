namespace OnlineCourses.Application.DTOs;

public class LessonDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateLessonDto
{
    public string Title { get; set; } = string.Empty;
}

public class UpdateLessonDto
{
    public string Title { get; set; } = string.Empty;
}

public class ReorderLessonDto
{
    public int NewOrder { get; set; }
}