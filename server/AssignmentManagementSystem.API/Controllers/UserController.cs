using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.DTOs.User;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Teacher")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Retrieves all active users, optionally filtered by Role (e.g. ?role=Teacher).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>> GetUsers([FromQuery] Role? role, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        if (role.HasValue)
        {
            users = users.Where(u => u.Role == role.Value);
        }

        var result = users.Where(u => u.IsActive).Select(u => new UserResponseDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role,
            ClassId = u.ClassId,
            IsActive = u.IsActive
        });

        return Ok(ApiResponse<IEnumerable<UserResponseDto>>.SuccessResponse(result, "Users retrieved successfully."));
    }
}
