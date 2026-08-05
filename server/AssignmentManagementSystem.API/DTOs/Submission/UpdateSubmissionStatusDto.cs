using System.ComponentModel.DataAnnotations;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.DTOs.Submission;

public class UpdateSubmissionStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    public SubmissionStatus Status { get; set; }
}
