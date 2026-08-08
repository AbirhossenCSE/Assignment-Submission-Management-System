using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class GradeSubmissionDto
{
    [Required(ErrorMessage = "Marks are required.")]
    [Range(0, 10000, ErrorMessage = "Marks must be between 0 and 10,000.")]
    public int Marks { get; set; }

    [StringLength(2000, ErrorMessage = "Feedback cannot exceed 2000 characters.")]
    public string? Feedback { get; set; }
}
