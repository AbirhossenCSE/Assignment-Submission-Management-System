using System.ComponentModel.DataAnnotations;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Assignment;

public class UpdateAssignmentDto
{
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string? Title { get; set; }

    [StringLength(5000, ErrorMessage = "Description cannot exceed 5000 characters.")]
    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    [Range(1, 10000, ErrorMessage = "MaxMarks must be between 1 and 10,000.")]
    public int? MaxMarks { get; set; }

    public AssignmentStatus? Status { get; set; }

    public bool? AllowResubmission { get; set; }
}
