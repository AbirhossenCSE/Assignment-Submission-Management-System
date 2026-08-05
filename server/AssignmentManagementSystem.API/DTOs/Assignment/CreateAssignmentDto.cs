using System.ComponentModel.DataAnnotations;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Assignment;

public class CreateAssignmentDto
{
    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClassId is required.")]
    public string ClassId { get; set; } = string.Empty;

    [Required(ErrorMessage = "SubjectId is required.")]
    public string SubjectId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Deadline is required.")]
    public DateTime Deadline { get; set; }

    [Range(1, 10000, ErrorMessage = "MaxMarks must be greater than 0.")]
    public int MaxMarks { get; set; } = 100;

    public bool AllowResubmission { get; set; } = true;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
}
