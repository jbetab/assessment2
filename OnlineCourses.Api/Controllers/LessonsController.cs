using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public LessonsController(ICourseRepository courseRepository, ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<LessonDto>>> GetByCourse(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);

        return Ok(lessons.Select(l => new LessonDto
        {
            Id = l.Id,
            CourseId = l.CourseId,
            Title = l.Title,
            Order = l.Order,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        }).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<LessonDto>> Create(Guid courseId, [FromBody] CreateLessonDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return NotFound();

        var existingLessons = await _lessonRepository.GetByCourseIdAsync(courseId);
        var nextOrder = existingLessons.Any() ? existingLessons.Max(l => l.Order) + 1 : 1;

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = dto.Title,
            Order = nextOrder,
            IsDeleted = false
        };

        var created = await _lessonRepository.CreateAsync(lesson);

        return CreatedAtAction(nameof(GetByCourse), new { courseId }, new LessonDto
        {
            Id = created.Id,
            CourseId = created.CourseId,
            Title = created.Title,
            Order = created.Order,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid courseId, Guid id, [FromBody] UpdateLessonDto dto)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        lesson.Title = dto.Title;
        await _lessonRepository.UpdateAsync(lesson);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid courseId, Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        await _lessonRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/reorder")]
    public async Task<IActionResult> Reorder(Guid courseId, Guid id, [FromBody] ReorderLessonDto dto)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null || lesson.CourseId != courseId)
            return NotFound();

        var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
        if (dto.NewOrder < 1 || dto.NewOrder > lessons.Count)
            return BadRequest(new { message = "Invalid order position" });

        await _lessonRepository.ReorderLessonsAsync(courseId, id, dto.NewOrder);

        return NoContent();
    }
}