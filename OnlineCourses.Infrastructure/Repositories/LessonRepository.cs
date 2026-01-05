using Microsoft.EntityFrameworkCore;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Infrastructure.Data;

namespace OnlineCourses.Infrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ApplicationDbContext _context;

    public LessonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _context.Lessons.FindAsync(id);
    }

    public async Task<List<Lesson>> GetByCourseIdAsync(Guid courseId)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }

    public async Task<Lesson> CreateAsync(Lesson lesson)
    {
        lesson.CreatedAt = DateTime.UtcNow;
        lesson.UpdatedAt = DateTime.UtcNow;
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        lesson.UpdatedAt = DateTime.UtcNow;
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null)
        {
            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> OrderExistsInCourse(Guid courseId, int order, Guid? excludeLessonId = null)
    {
        var query = _context.Lessons.Where(l => l.CourseId == courseId && l.Order == order);
        
        if (excludeLessonId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLessonId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task ReorderLessonsAsync(Guid courseId, Guid lessonId, int newOrder)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null) return;

        var oldOrder = lesson.Order;
        var lessons = await _context.Lessons
            .Where(l => l.CourseId == courseId && l.Id != lessonId)
            .OrderBy(l => l.Order)
            .ToListAsync();

        if (newOrder > oldOrder)
        {
            foreach (var l in lessons.Where(l => l.Order > oldOrder && l.Order <= newOrder))
            {
                l.Order--;
                l.UpdatedAt = DateTime.UtcNow;
            }
        }
        else if (newOrder < oldOrder)
        {
            foreach (var l in lessons.Where(l => l.Order >= newOrder && l.Order < oldOrder))
            {
                l.Order++;
                l.UpdatedAt = DateTime.UtcNow;
            }
        }

        lesson.Order = newOrder;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}