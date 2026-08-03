using AssignmentManagementSystem.API.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string SubjectId { get; set; } = string.Empty;
    public string CreatedByTeacherId { get; set; } = string.Empty;
    
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DueDate { get; set; }
    
    public int MaxScore { get; set; } = 100;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public List<string> AttachmentUrls { get; set; } = new();
}
