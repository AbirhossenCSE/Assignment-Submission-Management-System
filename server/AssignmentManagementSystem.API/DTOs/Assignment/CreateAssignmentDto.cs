using System.ComponentModel.DataAnnotations;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Assignment;

public class CreateAssignmentDto
{
    [Required(ErrorMessage = "Assignment title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assignment description is required.")]
    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Class ID is required.")]
    public string ClassId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject ID is required.")]
    public string SubjectId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Deadline is required.")]
    public DateTime Deadline { get; set; }

    [Required(ErrorMessage = "Maximum marks are required.")]
    [Range(1, 10000, ErrorMessage = "MaxMarks must be between 1 and 10,000.")]
    public int MaxMarks { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public bool AllowResubmission { get; set; } = true;
}
