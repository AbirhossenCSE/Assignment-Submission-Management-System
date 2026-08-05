using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class UpdateSubmissionDto
{
    [Required(ErrorMessage = "Answer text is required.")]
    public string AnswerText { get; set; } = string.Empty;

    public string? AttachmentUrl { get; set; }
}
