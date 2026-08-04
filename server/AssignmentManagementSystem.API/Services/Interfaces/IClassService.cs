using AssignmentManagementSystem.API.DTOs.Class;

namespace AssignmentManagementSystem.API.Services.Interfaces;

public interface IClassService
{
    Task<ClassResponseDto> CreateClassAsync(CreateClassDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(CancellationToken cancellationToken = default);
    Task<ClassResponseDto> GetClassByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ClassResponseDto> UpdateClassAsync(string id, UpdateClassDto dto, CancellationToken cancellationToken = default);
    Task DeleteClassAsync(string id, CancellationToken cancellationToken = default);
}
