using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class GradeSubmissionDto
{
    [Required(ErrorMessage = "Marks are required.")]
    [Range(0, 10000, ErrorMessage = "Marks must be 0 or greater.")]
    public int Marks { get; set; }

    public string? Feedback { get; set; }
}
