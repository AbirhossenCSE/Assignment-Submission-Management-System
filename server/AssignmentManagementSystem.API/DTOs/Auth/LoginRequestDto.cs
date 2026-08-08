using System.ComponentModel.DataAnnotations;

namespace AssignmentManagementSystem.API.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(150, ErrorMessage = "Email address cannot exceed 150 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
