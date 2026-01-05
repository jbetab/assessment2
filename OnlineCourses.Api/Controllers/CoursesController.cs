using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public CoursesController(ICourseRepository courseRepository, ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<CourseDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        CourseStatus? courseStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<CourseStatus>(status, true, out var parsedStatus))
        {
            courseStatus = parsedStatus;
        }

        var (items, totalCount) = await _courseRepository.SearchAsync(q, courseStatus, page, pageSize);

        var result = new PagedResultDto<CourseDto>
        {
            Items = items.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetById(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        return Ok(new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        });
    }

    [HttpGet("{id}/summary")]
    public async Task<ActionResult<CourseSummaryDto>> GetSummary(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        var activeLessons = course.Lessons.Count(l => !l.IsDeleted);
        var lastModified = course.Lessons.Any() 
            ? course.Lessons.Max(l => l.UpdatedAt) > course.UpdatedAt 
                ? course.Lessons.Max(l => l.UpdatedAt) 
                : course.UpdatedAt
            : course.UpdatedAt;

        return Ok(new CourseSummaryDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            TotalLessons = activeLessons,
            LastModified = lastModified
        });
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseDto dto)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Status = CourseStatus.Draft,
            IsDeleted = false
        };

        var created = await _courseRepository.CreateAsync(course);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new CourseDto
        {
            Id = created.Id,
            Title = created.Title,
            Status = created.Status.ToString(),
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        course.Title = dto.Title;
        await _courseRepository.UpdateAsync(course);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        await _courseRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        if (!course.CanBePublished())
        {
            return BadRequest(new { message = "Course must have at least one active lesson to be published" });
        }

        course.Status = CourseStatus.Published;
        await _courseRepository.UpdateAsync(course);

        return NoContent();
    }

    [HttpPatch("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            return NotFound();

        course.Status = CourseStatus.Draft;
        await _courseRepository.UpdateAsync(course);

        return NoContent();
    }
} 