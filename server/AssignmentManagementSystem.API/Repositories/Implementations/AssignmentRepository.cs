using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly IMongoCollection<Assignment> _assignmentsCollection;

    public AssignmentRepository(IMongoDatabase database)
    {
        _assignmentsCollection = database.GetCollection<Assignment>("Assignments");
    }

    public async Task<Assignment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _assignmentsCollection
            .Find(a => a.Id == id && !a.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _assignmentsCollection
            .Find(a => !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        return await _assignmentsCollection
            .Find(a => a.ClassId == classId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetPublishedByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        return await _assignmentsCollection
            .Find(a => a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _assignmentsCollection
            .Find(a => a.TeacherId == teacherId && !a.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _assignmentsCollection.InsertOneAsync(assignment, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(string id, Assignment assignment, CancellationToken cancellationToken = default)
    {
        assignment.UpdatedAt = DateTime.UtcNow;
        await _assignmentsCollection.ReplaceOneAsync(a => a.Id == id, assignment, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Assignment>.Update
            .Set(a => a.IsDeleted, true)
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        await _assignmentsCollection.UpdateOneAsync(a => a.Id == id, update, cancellationToken: cancellationToken);
    }
}
