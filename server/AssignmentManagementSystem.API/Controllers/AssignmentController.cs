using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Assignment;
using AssignmentManagementSystem.API.Helpers;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    /// <summary>
    /// Creates a new assignment. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssignmentResponseDto>>> CreateAssignment([FromBody] CreateAssignmentDto dto, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _assignmentService.CreateAssignmentAsync(teacherId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAssignmentById), new { id = result.Id }, ApiResponse<AssignmentResponseDto>.SuccessResponse(result, "Assignment created successfully."));
    }

    /// <summary>
    /// Updates an existing assignment created by the logged-in teacher. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AssignmentResponseDto>>> UpdateAssignment(string id, [FromBody] UpdateAssignmentDto dto, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _assignmentService.UpdateAssignmentAsync(id, teacherId, dto, cancellationToken);
        return Ok(ApiResponse<AssignmentResponseDto>.SuccessResponse(result, "Assignment updated successfully."));
    }

    /// <summary>
    /// Soft deletes an assignment created by the logged-in teacher. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAssignment(string id, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        await _assignmentService.DeleteAssignmentAsync(id, teacherId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Assignment deleted successfully."));
    }

    /// <summary>
    /// Publishes a draft assignment created by the logged-in teacher. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPatch("{id}/publish")]
    public async Task<ActionResult<ApiResponse<AssignmentResponseDto>>> PublishAssignment(string id, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _assignmentService.PublishAssignmentAsync(id, teacherId, cancellationToken);
        return Ok(ApiResponse<AssignmentResponseDto>.SuccessResponse(result, "Assignment published successfully."));
    }

    /// <summary>
    /// Retrieves all assignments created by the logged-in teacher. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentResponseDto>>>> GetMyAssignments(CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _assignmentService.GetAssignmentsByTeacherAsync(teacherId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AssignmentResponseDto>>.SuccessResponse(result, "Teacher assignments retrieved successfully."));
    }

    /// <summary>
    /// Retrieves assignments for a class (Students view published only; Teachers & Admins view all).
    /// </summary>
    [Authorize]
    [HttpGet("class/{classId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AssignmentResponseDto>>>> GetAssignmentsForClass(string classId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        if (userRole == null)
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("User role not recognized."));
        }

        var result = await _assignmentService.GetAssignmentsForClassAsync(classId, userId, userRole.Value, cancellationToken);
        return Ok(ApiResponse<IEnumerable<AssignmentResponseDto>>.SuccessResponse(result, "Class assignments retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a specific assignment by ID (Subject to role visibility rules).
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AssignmentResponseDto>>> GetAssignmentById(string id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        if (userRole == null)
        {
            return Unauthorized(ApiResponse<object>.FailureResponse("User role not recognized."));
        }

        var result = await _assignmentService.GetAssignmentByIdAsync(id, userId, userRole.Value, cancellationToken);
        return Ok(ApiResponse<AssignmentResponseDto>.SuccessResponse(result, "Assignment retrieved successfully."));
    }
}
