using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Subject;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    /// <summary>
    /// Creates a new Subject. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubjectResponseDto>>> CreateSubject([FromBody] CreateSubjectDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.CreateSubjectAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetSubjectById), new { id = result.Id }, ApiResponse<SubjectResponseDto>.SuccessResponse(result, "Subject created successfully."));
    }

    /// <summary>
    /// Retrieves all subjects. (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubjectResponseDto>>>> GetAllSubjects(CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetAllSubjectsAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<SubjectResponseDto>>.SuccessResponse(result, "Subjects retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a specific subject by ID. (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SubjectResponseDto>>> GetSubjectById(string id, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetSubjectByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SubjectResponseDto>.SuccessResponse(result, "Subject retrieved successfully."));
    }

    /// <summary>
    /// Retrieves all subjects for a specific class. (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet("class/{classId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SubjectResponseDto>>>> GetSubjectsByClass(string classId, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetSubjectsByClassAsync(classId, cancellationToken);
        return Ok(ApiResponse<IEnumerable<SubjectResponseDto>>.SuccessResponse(result, "Subjects retrieved successfully for class."));
    }

    /// <summary>
    /// Updates a subject by ID. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SubjectResponseDto>>> UpdateSubject(string id, [FromBody] UpdateSubjectDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.UpdateSubjectAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SubjectResponseDto>.SuccessResponse(result, "Subject updated successfully."));
    }

    /// <summary>
    /// Assigns a teacher to a subject. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/assign-teacher")]
    public async Task<ActionResult<ApiResponse<SubjectResponseDto>>> AssignTeacher(string id, [FromBody] AssignTeacherDto dto, CancellationToken cancellationToken)
    {
        var result = await _subjectService.AssignTeacherToSubjectAsync(id, dto.TeacherId, cancellationToken);
        return Ok(ApiResponse<SubjectResponseDto>.SuccessResponse(result, "Teacher assigned to subject successfully."));
    }

    /// <summary>
    /// Soft deletes a subject by ID. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSubject(string id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteSubjectAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Subject deleted successfully."));
    }
}
