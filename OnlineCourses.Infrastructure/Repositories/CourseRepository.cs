using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Infrastructure.Data;

namespace OnlineCourses.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .Include(c => c.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<(List<Course> Items, int TotalCount)> SearchAsync(
        string? searchTerm, CourseStatus? status, int page, int pageSize)
    {
        var query = _context.Courses.Include(c => c.Lessons).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Title.Contains(searchTerm));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Course> CreateAsync(Course course)
    {
        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}