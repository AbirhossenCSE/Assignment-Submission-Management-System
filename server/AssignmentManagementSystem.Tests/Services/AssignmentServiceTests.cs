using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.DTOs.Assignment;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssignmentManagementSystem.Tests.Services;

public class AssignmentServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IClassRepository> _classRepositoryMock;
    private readonly Mock<ISubjectRepository> _subjectRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<AssignmentService>> _loggerMock;
    private readonly AssignmentService _assignmentService;

    public AssignmentServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _classRepositoryMock = new Mock<IClassRepository>();
        _subjectRepositoryMock = new Mock<ISubjectRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AssignmentService>>();

        _assignmentService = new AssignmentService(
            _assignmentRepositoryMock.Object,
            _classRepositoryMock.Object,
            _subjectRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAssignment_WhenTeacherNotAssignedToSubject_ThrowsForbiddenException()
    {
        // Arrange
        string requestingTeacherId = "teacher-unassigned";
        var dto = new CreateAssignmentDto
        {
            Title = "Math Homework",
            Description = "Chapter 1 problems",
            ClassId = "class-101",
            SubjectId = "subject-101",
            Deadline = DateTime.UtcNow.AddDays(5),
            MaxMarks = 100
        };

        var classEntity = TestDataBuilder.CreateTestClass(id: dto.ClassId);
        var subject = TestDataBuilder.CreateTestSubject(id: dto.SubjectId, teacherId: "teacher-assigned-different");

        _classRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.ClassId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classEntity);

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);

        // Act
        Func<Task> action = async () => await _assignmentService.CreateAssignmentAsync(requestingTeacherId, dto);

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not assigned to teach this subject.");
    }

    [Fact]
    public async Task CreateAssignment_WhenDeadlineIsInPast_ThrowsBadRequestException()
    {
        // Arrange
        string teacherId = "teacher-101";
        var dto = new CreateAssignmentDto
        {
            Title = "Math Homework",
            Description = "Chapter 1 problems",
            ClassId = "class-101",
            SubjectId = "subject-101",
            Deadline = DateTime.UtcNow.AddDays(-1), // Past deadline!
            MaxMarks = 100
        };

        var classEntity = TestDataBuilder.CreateTestClass(id: dto.ClassId);
        var subject = TestDataBuilder.CreateTestSubject(id: dto.SubjectId, teacherId: teacherId);

        _classRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.ClassId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classEntity);

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);

        // Act
        Func<Task> action = async () => await _assignmentService.CreateAssignmentAsync(teacherId, dto);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Assignment deadline must be set to a future date and time.");
    }

    [Fact]
    public async Task CreateAssignment_WhenTeacherOwnsSubjectAndDeadlineIsValid_Succeeds()
    {
        // Arrange
        string teacherId = "teacher-101";
        var teacherUser = TestDataBuilder.CreateTestUser(id: teacherId, fullName: "Jane Teacher", role: Role.Teacher);
        var dto = new CreateAssignmentDto
        {
            Title = "Algebra Homework",
            Description = "Solve exercises 1-10",
            ClassId = "class-101",
            SubjectId = "subject-101",
            Deadline = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100
        };

        var classEntity = TestDataBuilder.CreateTestClass(id: dto.ClassId, name: "Class 10");
        var subject = TestDataBuilder.CreateTestSubject(id: dto.SubjectId, name: "Algebra", teacherId: teacherId);

        _classRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.ClassId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classEntity);

        _subjectRepositoryMock
            .Setup(r => r.GetByIdAsync(dto.SubjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subject);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacherUser);

        // Act
        var result = await _assignmentService.CreateAssignmentAsync(teacherId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.TeacherId.Should().Be(teacherId);
        result.ClassName.Should().Be(classEntity.Name);
        result.SubjectName.Should().Be(subject.Name);

        _assignmentRepositoryMock.Verify(r => r.CreateAsync(It.Is<Assignment>(a => a.TeacherId == teacherId && a.Title == dto.Title), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAssignment_WhenDifferentTeacherAttemptsUpdate_ThrowsForbiddenException()
    {
        // Arrange
        string ownerTeacherId = "teacher-owner";
        string attackerTeacherId = "teacher-attacker";
        var assignment = TestDataBuilder.CreateTestAssignment(teacherId: ownerTeacherId);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var updateDto = new UpdateAssignmentDto { Title = "Hacked Title" };

        // Act
        Func<Task> action = async () => await _assignmentService.UpdateAssignmentAsync(assignment.Id, attackerTeacherId, updateDto);

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to update this assignment.");
    }

    [Fact]
    public async Task DeleteAssignment_WhenDifferentTeacherAttemptsDelete_ThrowsForbiddenException()
    {
        // Arrange
        string ownerTeacherId = "teacher-owner";
        string attackerTeacherId = "teacher-attacker";
        var assignment = TestDataBuilder.CreateTestAssignment(teacherId: ownerTeacherId);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        // Act
        Func<Task> action = async () => await _assignmentService.DeleteAssignmentAsync(assignment.Id, attackerTeacherId);

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to delete this assignment.");
    }

    [Fact]
    public async Task PublishAssignment_WhenValid_ChangesStatusFromDraftToPublished()
    {
        // Arrange
        string teacherId = "teacher-101";
        var draftAssignment = TestDataBuilder.CreateTestAssignment(teacherId: teacherId, status: AssignmentStatus.Draft);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(draftAssignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftAssignment);

        // Act
        var result = await _assignmentService.PublishAssignmentAsync(draftAssignment.Id, teacherId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AssignmentStatus.Published);

        _assignmentRepositoryMock.Verify(r => r.UpdateAsync(draftAssignment.Id, It.Is<Assignment>(a => a.Status == AssignmentStatus.Published), It.IsAny<CancellationToken>()), Times.Once);
    }
}
