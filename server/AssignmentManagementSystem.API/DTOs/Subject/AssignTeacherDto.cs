using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Subject;

public class AssignTeacherDto
{
    [Required(ErrorMessage = "TeacherId is required.")]
    public string TeacherId { get; set; } = string.Empty;
}
