using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Common.Exceptions;
using AssignmentManagementSystem.API.DTOs.Submission;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using AssignmentManagementSystem.API.Services.Implementations;
using AssignmentManagementSystem.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssignmentManagementSystem.Tests.Services;

public class SubmissionServiceTests
{
    private readonly Mock<ISubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<SubmissionService>> _loggerMock;
    private readonly SubmissionService _submissionService;

    public SubmissionServiceTests()
    {
        _submissionRepositoryMock = new Mock<ISubmissionRepository>();
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<SubmissionService>>();

        _submissionService = new SubmissionService(
            _submissionRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SubmitAssignment_WhenAssignmentIsDraft_ThrowsBadRequestException()
    {
        // Arrange
        string studentId = "student-101";
        var draftAssignment = TestDataBuilder.CreateTestAssignment(status: AssignmentStatus.Draft);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(draftAssignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftAssignment);

        var dto = new SubmitAssignmentDto { AnswerText = "Draft answer" };

        // Act
        Func<Task> action = async () => await _submissionService.SubmitAssignmentAsync(draftAssignment.Id, studentId, dto);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Submissions are not accepted for draft assignments.");
    }

    [Fact]
    public async Task SubmitAssignment_WhenSubmissionAlreadyExists_ThrowsConflictException()
    {
        // Arrange
        string studentId = "student-101";
        var assignment = TestDataBuilder.CreateTestAssignment(status: AssignmentStatus.Published);
        var existingSubmission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id, studentId: studentId);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _submissionRepositoryMock
            .Setup(r => r.GetByAssignmentAndStudentAsync(assignment.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSubmission);

        var dto = new SubmitAssignmentDto { AnswerText = "Duplicate submission attempt" };

        // Act
        Func<Task> action = async () => await _submissionService.SubmitAssignmentAsync(assignment.Id, studentId, dto);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("A submission already exists for this assignment. Please use the resubmission endpoint to update your answer.");
    }

    [Fact]
    public async Task SubmitAssignment_WhenSubmittedBeforeDeadline_SetsSubmittedStatusAndIsLateFalse()
    {
        // Arrange
        string studentId = "student-101";
        var futureDeadline = DateTime.UtcNow.AddDays(3);
        var assignment = TestDataBuilder.CreateTestAssignment(status: AssignmentStatus.Published, deadline: futureDeadline);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _submissionRepositoryMock
            .Setup(r => r.GetByAssignmentAndStudentAsync(assignment.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Submission?)null);

        var dto = new SubmitAssignmentDto { AnswerText = "On-time calculus answer" };

        // Act
        var result = await _submissionService.SubmitAssignmentAsync(assignment.Id, studentId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.IsLate.Should().BeFalse();

        _submissionRepositoryMock.Verify(r => r.CreateAsync(
            It.Is<Submission>(s => s.Status == SubmissionStatus.Submitted && s.IsLate == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAssignment_WhenSubmittedAfterDeadline_SetsLateStatusAndIsLateTrue()
    {
        // Arrange
        string studentId = "student-101";
        var pastDeadline = DateTime.UtcNow.AddHours(-2); // Past deadline!
        var assignment = TestDataBuilder.CreateTestAssignment(status: AssignmentStatus.Published, deadline: pastDeadline);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _submissionRepositoryMock
            .Setup(r => r.GetByAssignmentAndStudentAsync(assignment.Id, studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Submission?)null);

        var dto = new SubmitAssignmentDto { AnswerText = "Late calculus answer" };

        // Act
        var result = await _submissionService.SubmitAssignmentAsync(assignment.Id, studentId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SubmissionStatus.Late);
        result.IsLate.Should().BeTrue();

        _submissionRepositoryMock.Verify(r => r.CreateAsync(
            It.Is<Submission>(s => s.Status == SubmissionStatus.Late && s.IsLate == true),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSubmission_WhenDeadlinePassed_ThrowsBadRequestException()
    {
        // Arrange
        string studentId = "student-101";
        var pastDeadline = DateTime.UtcNow.AddHours(-1);
        var assignment = TestDataBuilder.CreateTestAssignment(deadline: pastDeadline, allowResubmission: true);
        var submission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id, studentId: studentId, status: SubmissionStatus.Submitted);

        _submissionRepositoryMock
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var dto = new UpdateSubmissionDto { AnswerText = "Attempting update after deadline" };

        // Act
        Func<Task> action = async () => await _submissionService.UpdateSubmissionAsync(submission.Id, studentId, dto);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot update submission after deadline.");
    }

    [Fact]
    public async Task UpdateSubmission_WhenSubmissionIsGraded_ThrowsBadRequestException()
    {
        // Arrange
        string studentId = "student-101";
        var futureDeadline = DateTime.UtcNow.AddDays(2);
        var assignment = TestDataBuilder.CreateTestAssignment(deadline: futureDeadline, allowResubmission: true);
        var gradedSubmission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id, studentId: studentId, status: SubmissionStatus.Graded);

        _submissionRepositoryMock
            .Setup(r => r.GetByIdAsync(gradedSubmission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gradedSubmission);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var dto = new UpdateSubmissionDto { AnswerText = "Attempting update on graded submission" };

        // Act
        Func<Task> action = async () => await _submissionService.UpdateSubmissionAsync(gradedSubmission.Id, studentId, dto);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Cannot update a graded submission.");
    }

    [Fact]
    public async Task GradeSubmission_WhenTeacherDoesNotOwnAssignment_ThrowsForbiddenException()
    {
        // Arrange
        string ownerTeacherId = "teacher-owner";
        string attackerTeacherId = "teacher-attacker";

        var assignment = TestDataBuilder.CreateTestAssignment(teacherId: ownerTeacherId, maxMarks: 100);
        var submission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id);

        _submissionRepositoryMock
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var dto = new GradeSubmissionDto { Marks = 90, Feedback = "Good job" };

        // Act
        Func<Task> action = async () => await _submissionService.GradeSubmissionAsync(submission.Id, attackerTeacherId, dto);

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You are not authorized to grade submissions for this assignment.");
    }

    [Fact]
    public async Task GradeSubmission_WhenMarksExceedsMaxMarks_ThrowsBadRequestException()
    {
        // Arrange
        string teacherId = "teacher-owner";
        var assignment = TestDataBuilder.CreateTestAssignment(teacherId: teacherId, maxMarks: 100);
        var submission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id);

        _submissionRepositoryMock
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        var dto = new GradeSubmissionDto { Marks = 105, Feedback = "Bonus points!" }; // 105 > 100 MaxMarks!

        // Act
        Func<Task> action = async () => await _submissionService.GradeSubmissionAsync(submission.Id, teacherId, dto);

        // Assert
        await action.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Marks (105) cannot exceed the maximum allowed marks (100).");
    }

    [Fact]
    public async Task GradeSubmission_WhenValid_SetsGradedStatusMarksFeedbackAndGradedAt()
    {
        // Arrange
        string teacherId = "teacher-owner";
        var teacherUser = TestDataBuilder.CreateTestUser(id: teacherId, fullName: "Jane Teacher", role: Role.Teacher);
        var assignment = TestDataBuilder.CreateTestAssignment(teacherId: teacherId, maxMarks: 100);
        var submission = TestDataBuilder.CreateTestSubmission(assignmentId: assignment.Id, status: SubmissionStatus.Submitted);

        _submissionRepositoryMock
            .Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        _assignmentRepositoryMock
            .Setup(r => r.GetByIdAsync(assignment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignment);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(teacherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(teacherUser);

        var dto = new GradeSubmissionDto { Marks = 95, Feedback = "Excellent proof structure!" };

        // Act
        var result = await _submissionService.GradeSubmissionAsync(submission.Id, teacherId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SubmissionStatus.Graded);
        result.Marks.Should().Be(95);
        result.Feedback.Should().Be("Excellent proof structure!");
        result.GradedBy.Should().Be(teacherId);
        result.GradedByName.Should().Be(teacherUser.FullName);
        result.GradedAt.Should().NotBeNull();

        _submissionRepositoryMock.Verify(r => r.UpdateAsync(
            submission.Id,
            It.Is<Submission>(s => s.Status == SubmissionStatus.Graded && s.Marks == 95 && s.GradedBy == teacherId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
