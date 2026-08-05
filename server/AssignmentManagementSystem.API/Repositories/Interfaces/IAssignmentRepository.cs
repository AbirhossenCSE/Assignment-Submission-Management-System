using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.API.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assignment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Assignment>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assignment>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Assignment>> GetPublishedByClassIdAsync(string classId, CancellationToken cancellationToken = default);
    Task CreateAsync(Assignment assignment, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, Assignment assignment, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
