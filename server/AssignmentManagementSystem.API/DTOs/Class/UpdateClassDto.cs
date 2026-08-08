using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Class;

public class UpdateClassDto
{
    [Required(ErrorMessage = "Class name is required.")]
    [StringLength(100, ErrorMessage = "Class name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Section cannot exceed 20 characters.")]
    public string? Section { get; set; }

    public bool IsActive { get; set; } = true;
}
