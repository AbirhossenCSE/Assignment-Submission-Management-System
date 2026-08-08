using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class SubmitAssignmentDto
{
    [Required(ErrorMessage = "Answer text is required.")]
    [StringLength(10000, ErrorMessage = "Answer text cannot exceed 10,000 characters.")]
    public string AnswerText { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Attachment URL cannot exceed 2000 characters.")]
    public string? AttachmentUrl { get; set; }
}
