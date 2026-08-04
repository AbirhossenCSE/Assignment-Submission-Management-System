using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.API.Repositories.Interfaces;

public interface IClassRepository
{
    Task<ClassEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(ClassEntity classEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, ClassEntity classEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
