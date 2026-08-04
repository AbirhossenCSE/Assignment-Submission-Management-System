using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Subject;

public class UpdateSubjectDto
{
    [Required(ErrorMessage = "Subject Name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject Code is required.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "ClassId is required.")]
    public string ClassId { get; set; } = string.Empty;

    public string? TeacherId { get; set; }

    public bool IsActive { get; set; } = true;
}
