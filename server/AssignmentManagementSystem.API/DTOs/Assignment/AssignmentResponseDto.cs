using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Assignment;

public class AssignmentResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string ClassId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;

    public string SubjectId { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;

    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }
    public int MaxMarks { get; set; }
    public AssignmentStatus Status { get; set; }
    public bool AllowResubmission { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
