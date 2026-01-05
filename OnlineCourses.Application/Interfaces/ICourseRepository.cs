using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<(List<Course> Items, int TotalCount)> SearchAsync(string? searchTerm, CourseStatus? status, int page, int pageSize);
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Guid id);
}