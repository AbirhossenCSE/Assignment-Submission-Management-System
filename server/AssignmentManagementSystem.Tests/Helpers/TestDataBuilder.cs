using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.Tests.Helpers;

public static class TestDataBuilder
{
    public static User CreateTestUser(
        string id = "user-123",
        string fullName = "Test User",
        string email = "test@school.com",
        Role role = Role.Student,
        bool isActive = true)
    {
        return new User
        {
            Id = id,
            FullName = fullName,
            Email = email,
            PasswordHash = "hashed_secret_password",
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static ClassEntity CreateTestClass(
        string id = "class-123",
        string name = "Class 10",
        string section = "A",
        bool isActive = true)
    {
        return new ClassEntity
        {
            Id = id,
            Name = name,
            Section = section,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Subject CreateTestSubject(
        string id = "subject-123",
        string name = "Mathematics",
        string code = "MATH101",
        string classId = "class-123",
        string? teacherId = "teacher-123",
        bool isActive = true)
    {
        return new Subject
        {
            Id = id,
            Name = name,
            Code = code,
            ClassId = classId,
            TeacherId = teacherId,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Assignment CreateTestAssignment(
        string id = "assignment-123",
        string title = "Calculus Assignment",
        string description = "Solve chapter 3 problems.",
        string classId = "class-123",
        string subjectId = "subject-123",
        string teacherId = "teacher-123",
        DateTime? deadline = null,
        int maxMarks = 100,
        AssignmentStatus status = AssignmentStatus.Published,
        bool allowResubmission = true)
    {
        return new Assignment
        {
            Id = id,
            Title = title,
            Description = description,
            ClassId = classId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
            MaxMarks = maxMarks,
            Status = status,
            AllowResubmission = allowResubmission,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Submission CreateTestSubmission(
        string id = "submission-123",
        string assignmentId = "assignment-123",
        string studentId = "student-123",
        string answerText = "Here is my step by step answer.",
        string? attachmentUrl = "https://example.com/file.pdf",
        DateTime? submittedAt = null,
        SubmissionStatus status = SubmissionStatus.Submitted,
        int? marks = null,
        string? feedback = null,
        DateTime? gradedAt = null,
        string? gradedBy = null,
        bool isLate = false)
    {
        return new Submission
        {
            Id = id,
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = answerText,
            AttachmentUrl = attachmentUrl,
            SubmittedAt = submittedAt ?? DateTime.UtcNow,
            Status = status,
            Marks = marks,
            Feedback = feedback,
            GradedAt = gradedAt,
            GradedBy = gradedBy,
            IsLate = isLate,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
