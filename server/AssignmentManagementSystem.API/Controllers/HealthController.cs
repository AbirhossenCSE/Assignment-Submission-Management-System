using AssignmentManagementSystem.API.Common;
using AssignmentManagementSystem.API.DTOs.Common;
using AssignmentManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>
    /// Checks API running status and MongoDB database connectivity.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<HealthResponseDto>>> GetHealth(CancellationToken cancellationToken)
    {
        var result = await _healthService.GetHealthStatusAsync(cancellationToken);
        return Ok(ApiResponse<HealthResponseDto>.SuccessResponse(result, "Health check executed successfully."));
    }
}
