using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class ClassRepository : IClassRepository
{
    private readonly IMongoCollection<ClassEntity> _classesCollection;

    public ClassRepository(IMongoDatabase database)
    {
        _classesCollection = database.GetCollection<ClassEntity>("Classes");
    }

    public async Task<ClassEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _classesCollection
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ClassEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _classesCollection
            .Find(_ => true)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(ClassEntity classEntity, CancellationToken cancellationToken = default)
    {
        classEntity.CreatedAt = DateTime.UtcNow;
        classEntity.UpdatedAt = DateTime.UtcNow;
        await _classesCollection.InsertOneAsync(classEntity, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(string id, ClassEntity classEntity, CancellationToken cancellationToken = default)
    {
        classEntity.UpdatedAt = DateTime.UtcNow;
        await _classesCollection.ReplaceOneAsync(c => c.Id == id, classEntity, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<ClassEntity>.Update
            .Set(c => c.IsActive, false)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        await _classesCollection.UpdateOneAsync(c => c.Id == id, update, cancellationToken: cancellationToken);
    }
}
