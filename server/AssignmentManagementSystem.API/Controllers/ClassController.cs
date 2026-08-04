using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Class;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/classes")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    /// <summary>
    /// Creates a new Class/Course. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClassResponseDto>>> CreateClass([FromBody] CreateClassDto dto, CancellationToken cancellationToken)
    {
        var result = await _classService.CreateClassAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetClassById), new { id = result.Id }, ApiResponse<ClassResponseDto>.SuccessResponse(result, "Class created successfully."));
    }

    /// <summary>
    /// Retrieves all classes. (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClassResponseDto>>>> GetAllClasses(CancellationToken cancellationToken)
    {
        var result = await _classService.GetAllClassesAsync(cancellationToken);
        return Ok(ApiResponse<IEnumerable<ClassResponseDto>>.SuccessResponse(result, "Classes retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a specific class by ID. (Authenticated users)
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ClassResponseDto>>> GetClassById(string id, CancellationToken cancellationToken)
    {
        var result = await _classService.GetClassByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ClassResponseDto>.SuccessResponse(result, "Class retrieved successfully."));
    }

    /// <summary>
    /// Updates a class by ID. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ClassResponseDto>>> UpdateClass(string id, [FromBody] UpdateClassDto dto, CancellationToken cancellationToken)
    {
        var result = await _classService.UpdateClassAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<ClassResponseDto>.SuccessResponse(result, "Class updated successfully."));
    }

    /// <summary>
    /// Soft deletes a class by ID. (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteClass(string id, CancellationToken cancellationToken)
    {
        await _classService.DeleteClassAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Class deleted successfully."));
    }
}
