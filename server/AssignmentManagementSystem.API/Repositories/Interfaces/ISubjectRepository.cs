using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.API.Repositories.Interfaces;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Subject>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Subject>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default);
    Task CreateAsync(Subject subject, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, Subject subject, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
