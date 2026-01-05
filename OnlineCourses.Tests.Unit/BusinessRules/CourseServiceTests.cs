using FluentAssertions;
using Moq;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Application.Services;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Domain.Exeptions;
using Xunit;

namespace OnlineCourses.Tests.Unit.BusinessRules;

public class CourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepo = new();
    private readonly CourseService _service;

    public CourseServiceTests()
    {
        _service = new CourseService(_courseRepo.Object);
    }

    [Fact]
    public async Task PublishCourse_WithLessons_ShouldSucceed()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Test",
            Status = CourseStatus.Draft,
            IsDeleted = false,
            Lessons = new List<Lesson>
            {
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Title = "L1", Order = 1, IsDeleted = false }
            }
        };

        _courseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

        // Act
        await _service.PublishCourseAsync(courseId);

        // Assert
        course.Status.Should().Be(CourseStatus.Published);
        _courseRepo.Verify(r => r.UpdateAsync(course), Times.Once);
    }

    [Fact]
    public async Task PublishCourse_WithoutLessons_ShouldFail()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Test",
            Status = CourseStatus.Draft,
            IsDeleted = false,
            Lessons = new List<Lesson>() // sin lecciones activas
        };

        _courseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

        // Act
        var act = async () => await _service.PublishCourseAsync(courseId);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
        _courseRepo.Verify(r => r.UpdateAsync(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourse_ShouldBeSoftDelete()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var course = new Course
        {
            Id = courseId,
            Title = "Test",
            Status = CourseStatus.Draft,
            IsDeleted = false
        };

        _courseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

        // Act
        await _service.DeleteCourseSoftAsync(courseId);

        // Assert
        course.IsDeleted.Should().BeTrue();
        _courseRepo.Verify(r => r.UpdateAsync(course), Times.Once);

        // si tu ICourseRepository tiene DeleteAsync, validamos que NO se use:
        _courseRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
