using AssignmentManagementSystem.API.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class Submission : BaseEntity
{
    public string AssignmentId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string> AttachmentUrls { get; set; } = new();
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public double? Grade { get; set; }
    public string Feedback { get; set; } = string.Empty;
}
