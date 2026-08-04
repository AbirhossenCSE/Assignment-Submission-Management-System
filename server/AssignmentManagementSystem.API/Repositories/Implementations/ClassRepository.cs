using System.Collections.Concurrent;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class ClassRepository : IClassRepository
{
    private readonly IMongoCollection<ClassEntity>? _classesCollection;
    private static readonly ConcurrentDictionary<string, ClassEntity> _inMemoryClasses = new();
    private readonly ILogger<ClassRepository> _logger;
    private readonly bool _isMongoAvailable;

    public ClassRepository(IMongoDatabase database, ILogger<ClassRepository> logger)
    {
        _logger = logger;
        try
        {
            _classesCollection = database.GetCollection<ClassEntity>("Classes");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token);
            _isMongoAvailable = true;
        }
        catch
        {
            _isMongoAvailable = false;
            _logger.LogWarning("MongoDB connection unavailable. Operating in resilient dev mode for Classes.");
        }
    }

    public async Task<ClassEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _classesCollection != null)
        {
            try
            {
                return await _classesCollection.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Class.");
            }
        }

        _inMemoryClasses.TryGetValue(id, out var entity);
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<ClassEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _classesCollection != null)
        {
            try
            {
                return await _classesCollection.Find(_ => true).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Class.");
            }
        }

        return await Task.FromResult<IEnumerable<ClassEntity>>(_inMemoryClasses.Values);
    }

    public async Task CreateAsync(ClassEntity classEntity, CancellationToken cancellationToken = default)
    {
        classEntity.CreatedAt = DateTime.UtcNow;
        classEntity.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(classEntity.Id))
        {
            classEntity.Id = ObjectId.GenerateNewId().ToString();
        }

        if (_isMongoAvailable && _classesCollection != null)
        {
            try
            {
                await _classesCollection.InsertOneAsync(classEntity, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB insert failed, saving Class in memory.");
            }
        }

        _inMemoryClasses[classEntity.Id] = classEntity;
    }

    public async Task UpdateAsync(string id, ClassEntity classEntity, CancellationToken cancellationToken = default)
    {
        classEntity.UpdatedAt = DateTime.UtcNow;
        if (_isMongoAvailable && _classesCollection != null)
        {
            try
            {
                await _classesCollection.ReplaceOneAsync(c => c.Id == id, classEntity, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB update failed, updating Class in memory.");
            }
        }

        _inMemoryClasses[id] = classEntity;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _classesCollection != null)
        {
            try
            {
                await _classesCollection.DeleteOneAsync(c => c.Id == id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB delete failed, removing Class from memory.");
            }
        }

        _inMemoryClasses.TryRemove(id, out _);
    }
}
