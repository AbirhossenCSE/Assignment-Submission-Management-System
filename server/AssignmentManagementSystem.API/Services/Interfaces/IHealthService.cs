using AssignmentManagementSystem.API.DTOs.Common;

namespace AssignmentManagementSystem.API.Services.Interfaces;

public interface IHealthService
{
    Task<HealthResponseDto> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
