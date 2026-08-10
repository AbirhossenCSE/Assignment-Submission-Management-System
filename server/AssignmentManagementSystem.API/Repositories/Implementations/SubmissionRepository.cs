using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly IMongoCollection<Submission> _submissionsCollection;

    public SubmissionRepository(IMongoDatabase database)
    {
        _submissionsCollection = database.GetCollection<Submission>("Submissions");
    }

    public async Task<Submission?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Submission?> GetByAssignmentAndStudentAsync(string assignmentId, string studentId, CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(s => s.AssignmentId == assignmentId && s.StudentId == studentId && !s.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetByAssignmentIdAsync(string assignmentId, CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(s => s.AssignmentId == assignmentId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _submissionsCollection
            .Find(s => s.StudentId == studentId && !s.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        submission.CreatedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;
        await _submissionsCollection.InsertOneAsync(submission, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(string id, Submission submission, CancellationToken cancellationToken = default)
    {
        submission.UpdatedAt = DateTime.UtcNow;
        await _submissionsCollection.ReplaceOneAsync(s => s.Id == id, submission, cancellationToken: cancellationToken);
    }

    public async Task GradeAsync(string id, int marks, string? feedback, string gradedBy, CancellationToken cancellationToken = default)
    {
        var update = Builders<Submission>.Update
            .Set(s => s.Marks, marks)
            .Set(s => s.Feedback, feedback)
            .Set(s => s.GradedBy, gradedBy)
            .Set(s => s.GradedAt, DateTime.UtcNow)
            .Set(s => s.Status, SubmissionStatus.Graded)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        await _submissionsCollection.UpdateOneAsync(s => s.Id == id, update, cancellationToken: cancellationToken);
    }

    public async Task UpdateStatusAsync(string id, SubmissionStatus status, CancellationToken cancellationToken = default)
    {
        var update = Builders<Submission>.Update
            .Set(s => s.Status, status)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        await _submissionsCollection.UpdateOneAsync(s => s.Id == id, update, cancellationToken: cancellationToken);
    }
}
