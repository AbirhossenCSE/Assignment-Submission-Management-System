using System.Collections.Concurrent;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly IMongoCollection<Submission>? _submissionsCollection;
    private static readonly ConcurrentDictionary<string, Submission> _inMemorySubmissions = new();
    private readonly ILogger<SubmissionRepository> _logger;
    private readonly bool _isMongoAvailable;

    public SubmissionRepository(IMongoDatabase database, ILogger<SubmissionRepository> logger)
    {
        _logger = logger;
        try
        {
            _submissionsCollection = database.GetCollection<Submission>("Submissions");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token);
            _isMongoAvailable = true;
        }
        catch
        {
            _isMongoAvailable = false;
            _logger.LogWarning("MongoDB connection unavailable. Operating in resilient dev mode for Submissions.");
        }
    }

    public async Task<Submission?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                return await _submissionsCollection.Find(s => s.Id == id && !s.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Submission.");
            }
        }

        _inMemorySubmissions.TryGetValue(id, out var entity);
        if (entity != null && entity.IsDeleted) return null;
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<Submission>> GetByAssignmentIdAsync(string assignmentId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                return await _submissionsCollection.Find(s => s.AssignmentId == assignmentId && !s.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Submission.");
            }
        }

        return await Task.FromResult(_inMemorySubmissions.Values.Where(s => s.AssignmentId == assignmentId && !s.IsDeleted));
    }

    public async Task<IEnumerable<Submission>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                return await _submissionsCollection.Find(s => s.StudentId == studentId && !s.IsDeleted).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Submission.");
            }
        }

        return await Task.FromResult(_inMemorySubmissions.Values.Where(s => s.StudentId == studentId && !s.IsDeleted));
    }

    public async Task<Submission?> GetByAssignmentAndStudentAsync(string assignmentId, string studentId, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                return await _submissionsCollection
                    .Find(s => s.AssignmentId == assignmentId && s.StudentId == studentId && !s.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store for Submission.");
            }
        }

        var entity = _inMemorySubmissions.Values.FirstOrDefault(s => s.AssignmentId == assignmentId && s.StudentId == studentId && !s.IsDeleted);
        return await Task.FromResult(entity);
    }

    public async Task CreateAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        submission.CreatedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(submission.Id))
        {
            submission.Id = ObjectId.GenerateNewId().ToString();
        }

        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                await _submissionsCollection.InsertOneAsync(submission, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB insert failed, saving Submission in memory.");
            }
        }

        _inMemorySubmissions[submission.Id] = submission;
    }

    public async Task UpdateAsync(string id, Submission submission, CancellationToken cancellationToken = default)
    {
        submission.UpdatedAt = DateTime.UtcNow;
        if (_isMongoAvailable && _submissionsCollection != null)
        {
            try
            {
                await _submissionsCollection.ReplaceOneAsync(s => s.Id == id, submission, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB update failed, updating Submission in memory.");
            }
        }

        _inMemorySubmissions[id] = submission;
    }
}
