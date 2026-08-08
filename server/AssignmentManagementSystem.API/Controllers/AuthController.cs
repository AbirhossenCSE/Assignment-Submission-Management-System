using System.Security.Claims;
using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Auth;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user (Admin, Teacher, or Student).
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully."));
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful."));
    }

    /// <summary>
    /// Protected endpoint returning the authenticated user's claims.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var classId = User.FindFirstValue("classId");

        var userInfo = new
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            Role = role,
            ClassId = classId
        };

        return Ok(ApiResponse<object>.SuccessResponse(userInfo, "User claims retrieved successfully."));
    }

    /// <summary>
    /// Test endpoint accessible only by Admin users.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public ActionResult<ApiResponse<string>> AdminOnlyEndpoint()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted to Admin role."));
    }

    /// <summary>
    /// Test endpoint accessible only by Teacher users.
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpGet("teacher-only")]
    public ActionResult<ApiResponse<string>> TeacherOnlyEndpoint()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted to Teacher role."));
    }

    /// <summary>
    /// Test endpoint accessible only by Student users.
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("student-only")]
    public ActionResult<ApiResponse<string>> StudentOnlyEndpoint()
    {
        return Ok(ApiResponse<string>.SuccessResponse("Access granted to Student role."));
    }
}
