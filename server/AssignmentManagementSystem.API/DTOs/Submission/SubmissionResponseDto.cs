using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class SubmissionResponseDto
{
    public string Id { get; set; } = string.Empty;

    public string AssignmentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public int MaxMarks { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }

    public int? Marks { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? GradedBy { get; set; }
    public string? GradedByName { get; set; }

    public bool IsLate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
