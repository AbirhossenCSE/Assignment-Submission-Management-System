using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.API.Repositories.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Submission>> GetByAssignmentIdAsync(string assignmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Submission>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);
    Task<Submission?> GetByAssignmentAndStudentAsync(string assignmentId, string studentId, CancellationToken cancellationToken = default);
    Task CreateAsync(Submission submission, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, Submission submission, CancellationToken cancellationToken = default);
}
