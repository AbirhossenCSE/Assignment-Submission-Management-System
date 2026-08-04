using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class Subject : BaseEntity
{
    [BsonRequired]
    public string Name { get; set; } = string.Empty;

    [BsonRequired]
    public string Code { get; set; } = string.Empty;

    [BsonRequired]
    public string ClassId { get; set; } = string.Empty;

    public string? TeacherId { get; set; }

    public bool IsActive { get; set; } = true;
}
