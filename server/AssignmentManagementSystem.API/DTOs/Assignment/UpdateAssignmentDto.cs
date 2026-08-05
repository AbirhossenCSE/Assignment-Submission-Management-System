using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Assignment;

public class UpdateAssignmentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ClassId { get; set; }
    public string? SubjectId { get; set; }
    public DateTime? Deadline { get; set; }
    public int? MaxMarks { get; set; }
    public bool? AllowResubmission { get; set; }
    public AssignmentStatus? Status { get; set; }
}
