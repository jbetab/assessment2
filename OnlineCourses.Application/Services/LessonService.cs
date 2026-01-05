using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Exeptions;

namespace OnlineCourses.Application.Services;

public class LessonService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public LessonService(ICourseRepository courseRepository, ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }

    /// <summary>
    /// Regla: Order debe ser ÚNICO por curso (entre lecciones NO eliminadas).
    /// </summary>
    public async Task<Lesson> CreateLessonAsync(Guid courseId, string title, int order)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new KeyNotFoundException("Course not found");

        var existing = await _lessonRepository.GetByCourseIdAsync(courseId);
        var active = existing.Where(l => !l.IsDeleted).ToList();

        if (active.Any(l => l.Order == order))
            throw new BusinessRuleException($"Lesson order '{order}' is already used in this course");

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title,
            Order = order,
            IsDeleted = false
        };

        return await _lessonRepository.CreateAsync(lesson);
    }
}