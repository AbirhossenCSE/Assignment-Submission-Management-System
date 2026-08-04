using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Class;

public class CreateClassDto
{
    [Required(ErrorMessage = "Class Name is required.")]
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }
}
