using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Subject;

public class UpdateSubjectDto
{
    [Required(ErrorMessage = "Subject name is required.")]
    [StringLength(100, ErrorMessage = "Subject name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subject code is required.")]
    [StringLength(20, ErrorMessage = "Subject code cannot exceed 20 characters.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Class ID is required.")]
    public string ClassId { get; set; } = string.Empty;

    public string? TeacherId { get; set; }

    public bool IsActive { get; set; } = true;
}
