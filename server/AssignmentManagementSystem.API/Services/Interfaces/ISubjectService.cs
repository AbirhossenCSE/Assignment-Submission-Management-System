using AssignmentManagementSystem.API.DTOs.Subject;

namespace AssignmentManagementSystem.API.Services.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync(CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> GetSubjectByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubjectResponseDto>> GetSubjectsByClassAsync(string classId, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> UpdateSubjectAsync(string id, UpdateSubjectDto dto, CancellationToken cancellationToken = default);
    Task<SubjectResponseDto> AssignTeacherToSubjectAsync(string subjectId, string teacherId, CancellationToken cancellationToken = default);
    Task DeleteSubjectAsync(string id, CancellationToken cancellationToken = default);
}
