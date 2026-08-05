using AssignmentManagementSystem.API.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class Assignment : BaseEntity
{
    [BsonRequired]
    public string Title { get; set; } = string.Empty;

    [BsonRequired]
    public string Description { get; set; } = string.Empty;

    [BsonRequired]
    public string ClassId { get; set; } = string.Empty;

    [BsonRequired]
    public string SubjectId { get; set; } = string.Empty;

    [BsonRequired]
    public string TeacherId { get; set; } = string.Empty;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Deadline { get; set; }

    public int MaxMarks { get; set; } = 100;

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public bool AllowResubmission { get; set; } = true;

    public bool IsDeleted { get; set; } = false;
}
