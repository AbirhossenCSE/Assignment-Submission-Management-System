using System.Collections.Concurrent;
using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly IMongoCollection<Assignment>? _assignmentsCollection;
    private static readonly ConcurrentDictionary<string, Assignment> _inMemoryAssignments = new();
    private readonly ILogger<AssignmentRepository> _logger;
    private readonly bool _isMongoAvailable;

    public AssignmentRepository(IMongoDatabase database, ILogger<AssignmentRepository> logger)
    {
        _logger = logger;
        try
        {
            _assignmentsCollection = database.GetCollection<Assignment>("Assignments");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token);
            _isMongoAvailable = true;
        }
        catch
        {
            _isMongoAvailable = false;
            _logger.LogWarning("MongoDB connection unavailable. Operating in resilient dev mode for Assignments.");
        }
    }

    public async Task<Assignment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                return await _assignmentsCollection.Find(a => a.Id == id && !a.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Assignment.");
            }
        }

        _inMemoryAssignments.TryGetValue(id, out var entity);
        if (entity != null && entity.IsDeleted) return null;
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<Assignment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                return await _assignmentsCollection.Find(a => !a.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Assignment.");
            }
        }

        return await Task.FromResult(_inMemoryAssignments.Values.Where(a => !a.IsDeleted));
    }

    public async Task<IEnumerable<Assignment>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                return await _assignmentsCollection.Find(a => a.TeacherId == teacherId && !a.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Assignment.");
            }
        }

        return await Task.FromResult(_inMemoryAssignments.Values.Where(a => a.TeacherId == teacherId && !a.IsDeleted));
    }

    public async Task<IEnumerable<Assignment>> GetByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                return await _assignmentsCollection.Find(a => a.ClassId == classId && !a.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Assignment.");
            }
        }

        return await Task.FromResult(_inMemoryAssignments.Values.Where(a => a.ClassId == classId && !a.IsDeleted));
    }

    public async Task<IEnumerable<Assignment>> GetPublishedByClassIdAsync(string classId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                return await _assignmentsCollection.Find(a => a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Assignment.");
            }
        }

        return await Task.FromResult(_inMemoryAssignments.Values.Where(a => a.ClassId == classId && a.Status == AssignmentStatus.Published && !a.IsDeleted));
    }

    public async Task CreateAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(assignment.Id))
        {
            assignment.Id = ObjectId.GenerateNewId().ToString();
        }

        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                await _assignmentsCollection.InsertOneAsync(assignment, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB insert failed, saving Assignment in memory.");
            }
        }

        _inMemoryAssignments[assignment.Id] = assignment;
    }

    public async Task UpdateAsync(string id, Assignment assignment, CancellationToken cancellationToken = default)
    {
        assignment.UpdatedAt = DateTime.UtcNow;
        if (_isMongoAvailable && _assignmentsCollection != null)
        {
            try
            {
                await _assignmentsCollection.ReplaceOneAsync(a => a.Id == id, assignment, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB update failed, updating Assignment in memory.");
            }
        }

        _inMemoryAssignments[id] = assignment;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var assignment = await GetByIdAsync(id, cancellationToken);
        if (assignment != null)
        {
            assignment.IsDeleted = true;
            await UpdateAsync(id, assignment, cancellationToken);
        }
    }
}
