using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Submission;
using AssignmentManagementSystem.API.Helpers;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api")]
public class SubmissionController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    /// <summary>
    /// Submits an answer for a published assignment. (Student only)
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPost("assignments/{assignmentId}/submissions")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto>>> SubmitAssignment(string assignmentId, [FromBody] SubmitAssignmentDto dto, CancellationToken cancellationToken)
    {
        var studentId = User.GetUserId();
        var result = await _submissionService.SubmitAssignmentAsync(assignmentId, studentId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetSubmissionById), new { id = result.Id }, ApiResponse<SubmissionResponseDto>.SuccessResponse(result, "Assignment submitted successfully."));
    }

    /// <summary>
    /// Resubmits/updates an answer before the deadline. (Student only)
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpPut("submissions/{id}")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto>>> UpdateSubmission(string id, [FromBody] UpdateSubmissionDto dto, CancellationToken cancellationToken)
    {
        var studentId = User.GetUserId();
        var result = await _submissionService.UpdateSubmissionAsync(id, studentId, dto, cancellationToken);
        return Ok(ApiResponse<SubmissionResponseDto>.SuccessResponse(result, "Submission updated successfully."));
    }

    /// <summary>
    /// Grades a student submission with marks and feedback. (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPatch("submissions/{id}/grade")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto>>> GradeSubmission(string id, [FromBody] GradeSubmissionDto dto, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _submissionService.GradeSubmissionAsync(id, teacherId, dto, cancellationToken);
        return Ok(ApiResponse<SubmissionResponseDto>.SuccessResponse(result, "Submission graded successfully."));
    }

    /// <summary>
    /// Manually updates a submission's status (e.g. requesting resubmission). (Teacher only)
    /// </summary>
    [Authorize(Roles = "Teacher")]
    [HttpPatch("submissions/{id}/status")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto>>> UpdateSubmissionStatus(string id, [FromBody] UpdateSubmissionStatusDto dto, CancellationToken cancellationToken)
    {
        var teacherId = User.GetUserId();
        var result = await _submissionService.UpdateSubmissionStatusAsync(id, teacherId, dto.Status, cancellationToken);
        return Ok(ApiResponse<SubmissionResponseDto>.SuccessResponse(result, "Submission status updated successfully."));
    }

    /// <summary>
    /// Retrieves all submissions for a specific assignment. (Teacher & Admin only)
    /// </summary>
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("assignments/{assignmentId}/submissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubmissionResponseDto>>>> GetSubmissionsForAssignment(string assignmentId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        if (userRole == null) return Unauthorized(ApiResponse<object>.FailureResponse("User role not recognized."));

        var result = await _submissionService.GetSubmissionsForAssignmentAsync(assignmentId, userId, userRole.Value, cancellationToken);
        return Ok(ApiResponse<IEnumerable<SubmissionResponseDto>>.SuccessResponse(result, "Submissions retrieved successfully."));
    }

    /// <summary>
    /// Retrieves the logged-in student's submission history across all assignments. (Student only)
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("submissions/my")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubmissionResponseDto>>>> GetMySubmissions(CancellationToken cancellationToken)
    {
        var studentId = User.GetUserId();
        var result = await _submissionService.GetMySubmissionsAsync(studentId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<SubmissionResponseDto>>.SuccessResponse(result, "My submissions retrieved successfully."));
    }

    /// <summary>
    /// Checks the student's submission status for a specific assignment. (Student only)
    /// </summary>
    [Authorize(Roles = "Student")]
    [HttpGet("assignments/{assignmentId}/my-submission")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto?>>> GetMySubmissionForAssignment(string assignmentId, CancellationToken cancellationToken)
    {
        var studentId = User.GetUserId();
        var result = await _submissionService.GetSubmissionStatusForStudentAsync(assignmentId, studentId, cancellationToken);
        if (result == null)
        {
            return Ok(ApiResponse<SubmissionResponseDto?>.SuccessResponse(null, "No submission exists for this assignment yet."));
        }
        return Ok(ApiResponse<SubmissionResponseDto?>.SuccessResponse(result, "Submission retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a submission by ID (Subject to role visibility rules). (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet("submissions/{id}")]
    public async Task<ActionResult<ApiResponse<SubmissionResponseDto>>> GetSubmissionById(string id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var userRole = User.GetUserRole();
        if (userRole == null) return Unauthorized(ApiResponse<object>.FailureResponse("User role not recognized."));

        var result = await _submissionService.GetSubmissionByIdAsync(id, userId, userRole.Value, cancellationToken);
        return Ok(ApiResponse<SubmissionResponseDto>.SuccessResponse(result, "Submission retrieved successfully."));
    }
}
