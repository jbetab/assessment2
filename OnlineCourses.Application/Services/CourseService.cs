using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Domain.Exeptions;

namespace OnlineCourses.Application.Services;

public class CourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task PublishCourseAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new KeyNotFoundException("Course not found");

        if (!course.CanBePublished())
            throw new BusinessRuleException("Course must have at least one active lesson to be published");

        course.Status = CourseStatus.Published;
        await _courseRepository.UpdateAsync(course);
    }

    public async Task DeleteCourseSoftAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            throw new KeyNotFoundException("Course not found");

        // soft delete (regla de negocio)
        course.IsDeleted = true;
        await _courseRepository.UpdateAsync(course);

        // IMPORTANTE: no llamamos DeleteAsync aquí.
    }
}