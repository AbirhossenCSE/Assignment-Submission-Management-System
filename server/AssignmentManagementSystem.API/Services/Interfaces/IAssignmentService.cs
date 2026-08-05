using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.DTOs.Assignment;

namespace AssignmentManagementSystem.API.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponseDto> CreateAssignmentAsync(string teacherId, CreateAssignmentDto dto, CancellationToken cancellationToken = default);
    Task<AssignmentResponseDto> UpdateAssignmentAsync(string assignmentId, string teacherId, UpdateAssignmentDto dto, CancellationToken cancellationToken = default);
    Task DeleteAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default);
    Task<AssignmentResponseDto> PublishAssignmentAsync(string assignmentId, string teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsByTeacherAsync(string teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AssignmentResponseDto>> GetAssignmentsForClassAsync(string classId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default);
    Task<AssignmentResponseDto> GetAssignmentByIdAsync(string assignmentId, string requestingUserId, Role requestingUserRole, CancellationToken cancellationToken = default);
}
