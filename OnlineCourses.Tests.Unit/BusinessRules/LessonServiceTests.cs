using FluentAssertions;
using Moq;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Application.Services;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Exeptions;
using Xunit;

namespace OnlineCourses.Tests.Unit.BusinessRules;

public class LessonServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepo = new();
    private readonly Mock<ILessonRepository> _lessonRepo = new();
    private readonly LessonService _service;

    public LessonServiceTests()
    {
        _service = new LessonService(_courseRepo.Object, _lessonRepo.Object);
    }

    [Fact]
    public async Task CreateLesson_WithUniqueOrder_ShouldSucceed()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _courseRepo.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(new Course { Id = courseId, Lessons = new List<Lesson>() });

        _lessonRepo.Setup(r => r.GetByCourseIdAsync(courseId))
            .ReturnsAsync(new List<Lesson>
            {
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 1, IsDeleted = false },
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 2, IsDeleted = false },
            });

        Lesson? createdArg = null;
        _lessonRepo.Setup(r => r.CreateAsync(It.IsAny<Lesson>()))
            .Callback<Lesson>(l => createdArg = l)
            .ReturnsAsync((Lesson l) => l);

        // Act
        var created = await _service.CreateLessonAsync(courseId, "New Lesson", 3);

        // Assert
        created.Order.Should().Be(3);
        created.Title.Should().Be("New Lesson");
        created.CourseId.Should().Be(courseId);

        createdArg.Should().NotBeNull();
        createdArg!.Order.Should().Be(3);

        _lessonRepo.Verify(r => r.CreateAsync(It.IsAny<Lesson>()), Times.Once);
    }

    [Fact]
    public async Task CreateLesson_WithDuplicateOrder_ShouldFail()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _courseRepo.Setup(r => r.GetByIdAsync(courseId))
            .ReturnsAsync(new Course { Id = courseId, Lessons = new List<Lesson>() });

        _lessonRepo.Setup(r => r.GetByCourseIdAsync(courseId))
            .ReturnsAsync(new List<Lesson>
            {
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 1, IsDeleted = false },
                new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 2, IsDeleted = false },
            });

        // Act
        var act = async () => await _service.CreateLessonAsync(courseId, "Dup", 2);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>();
        _lessonRepo.Verify(r => r.CreateAsync(It.IsAny<Lesson>()), Times.Never);
    }
}
