using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssignmentManagementSystem.Tests.Services;

public class SubjectServiceTests
{
    private readonly Mock<ISubjectRepository> _subjectRepositoryMock;
    private readonly Mock<IClassRepository> _classRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<SubjectService>> _loggerMock;
    private readonly SubjectService _subjectService;

    public SubjectServiceTests()
    {
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _classRepositoryMock = new Mock<IClassRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<SubjectService>>();

        _subjectService = new SubjectService(
            _subjectRepositoryMock.Object,
            _classRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AssignTeacherToSubject_WhenSubjectDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        string subjectId = "non-existent-subject";
        string teacherId = "teacher-123";

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subject?)null);

        // Act
        Func<Task> action = async () => await _subjectService.AssignTeacherToSubjectAsync(subjectId, teacherId);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Subject with ID '{subjectId}' was not found.");
    }

    [Fact]
    public async Task AssignTeacherToSubject_WhenTargetUserIsNotTeacherRole_ThrowsBadRequestException()
    {
        // Arrange
        var subject = TestDataBuilder.CreateTestSubject(id: "subj-101");
        var studentUser = TestDataBuilder.CreateTestUser(id: "student-456", fullName: "John Student", role: Role.Student);

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(subject.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(studentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(studentUser);

        // Act
        Func<Task> action = async () => await _subjectService.AssignTeacherToSubjectAsync(subject.Id, studentUser.Id);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage($"User '{studentUser.FullName}' does not have the Teacher role.");
    }

    [Fact]
    public async Task AssignTeacherToSubject_WhenUserIsValidTeacher_Succeeds()
    {
        // Arrange
        var subject = TestDataBuilder.CreateTestSubject(id: "subj-101");
        var teacherUser = TestDataBuilder.CreateTestUser(id: "teacher-789", fullName: "Jane Teacher", role: Role.Teacher);
        var classEntity = TestDataBuilder.CreateTestClass(id: subject.ClassId, name: "Class 10");

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(subject.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(teacherUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacherUser);

        _classRepositoryMock
            .Setup(r => r.GetByIdAsync(subject.ClassId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classEntity);

        // Act
        var result = await _subjectService.AssignTeacherToSubjectAsync(subject.Id, teacherUser.Id);

        // Assert
        result.Should().NotBeNull();
        result.TeacherId.Should().Be(teacherUser.Id);
        result.TeacherName.Should().Be(teacherUser.FullName);

        _subjectRepositoryMock.Verify(r => r.UpdateAsync(subject.Id, It.Is<Subject>(s => s.TeacherId == teacherUser.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
