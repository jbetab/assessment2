using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Application.Interfaces;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<List<Lesson>> GetByCourseIdAsync(Guid courseId);
    Task<Lesson> CreateAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(Guid id);
    Task<bool> OrderExistsInCourse(Guid courseId, int order, Guid? excludeLessonId = null);
    Task ReorderLessonsAsync(Guid courseId, Guid lessonId, int newOrder);
}