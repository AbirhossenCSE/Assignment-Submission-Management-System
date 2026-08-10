using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly IMongoCollection<Subject> _subjectsCollection;

    public SubjectRepository(IMongoDatabase database)
    {
        _subjectsCollection = database.GetCollection<Subject>("Subjects");
    }

    public async Task<Subject?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _subjectsCollection
            .Find(s => s.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _subjectsCollection
            .Find(_ => true)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        return await _subjectsCollection
            .Find(s => s.ClassId == classId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _subjectsCollection
            .Find(s => s.TeacherId == teacherId)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        subject.CreatedAt = DateTime.UtcNow;
        subject.UpdatedAt = DateTime.UtcNow;
        await _subjectsCollection.InsertOneAsync(subject, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(string id, Subject subject, CancellationToken cancellationToken = default)
    {
        subject.UpdatedAt = DateTime.UtcNow;
        await _subjectsCollection.ReplaceOneAsync(s => s.Id == id, subject, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Subject>.Update
            .Set(s => s.IsActive, false)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        await _subjectsCollection.UpdateOneAsync(s => s.Id == id, update, cancellationToken: cancellationToken);
    }
}
