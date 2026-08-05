using AssignmentManagementSystem.API.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class Submission : BaseEntity
{
    [BsonRequired]
    public string AssignmentId { get; set; } = string.Empty;

    [BsonRequired]
    public string StudentId { get; set; } = string.Empty;

    [BsonRequired]
    public string AnswerText { get; set; } = string.Empty;

    public string? AttachmentUrl { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? SubmittedAt { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

    public int? Marks { get; set; }

    public string? Feedback { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? GradedAt { get; set; }

    public string? GradedBy { get; set; }

    public bool IsLate { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
}
