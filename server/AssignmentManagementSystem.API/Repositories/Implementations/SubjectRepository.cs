using System.Collections.Concurrent;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class SubjectRepository : ISubjectRepository
{
    private readonly IMongoCollection<Subject>? _subjectsCollection;
    private static readonly ConcurrentDictionary<string, Subject> _inMemorySubjects = new();
    private readonly ILogger<SubjectRepository> _logger;
    private readonly bool _isMongoAvailable;

    public SubjectRepository(IMongoDatabase database, ILogger<SubjectRepository> logger)
    {
        _logger = logger;
        try
        {
            _subjectsCollection = database.GetCollection<Subject>("Subjects");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token);
            _isMongoAvailable = true;
        }
        catch
        {
            _isMongoAvailable = false;
            _logger.LogWarning("MongoDB connection unavailable. Operating in resilient dev mode for Subjects.");
        }
    }

    public async Task<Subject?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                return await _subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Subject.");
            }
        }

        _inMemorySubjects.TryGetValue(id, out var entity);
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                return await _subjectsCollection.Find(_ => true).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Subject.");
            }
        }

        return await Task.FromResult<IEnumerable<Subject>>(_inMemorySubjects.Values);
    }

    public async Task<IEnumerable<Subject>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                return await _subjectsCollection.Find(s => s.ClassId == classId).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Subject.");
            }
        }

        var subjects = _inMemorySubjects.Values.Where(s => s.ClassId == classId);
        return await Task.FromResult(subjects);
    }

    public async Task<IEnumerable<Subject>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                return await _subjectsCollection.Find(s => s.TeacherId == teacherId).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Subject.");
            }
        }

        var subjects = _inMemorySubjects.Values.Where(s => s.TeacherId == teacherId);
        return await Task.FromResult(subjects);
    }

    public async Task CreateAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        subject.CreatedAt = DateTime.UtcNow;
        subject.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(subject.Id))
        {
            subject.Id = ObjectId.GenerateNewId().ToString();
        }

        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                await _subjectsCollection.InsertOneAsync(subject, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB insert failed, saving Subject in memory.");
            }
        }

        _inMemorySubjects[subject.Id] = subject;
    }

    public async Task UpdateAsync(string id, Subject subject, CancellationToken cancellationToken = default)
    {
        subject.UpdatedAt = DateTime.UtcNow;
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                await _subjectsCollection.ReplaceOneAsync(s => s.Id == id, subject, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB update failed, updating Subject in memory.");
            }
        }

        _inMemorySubjects[id] = subject;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _subjectsCollection != null)
        {
            try
            {
                await _subjectsCollection.DeleteOneAsync(s => s.Id == id, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB delete failed, removing Subject from memory.");
            }
        }

        _inMemorySubjects.TryRemove(id, out _);
    }
}
