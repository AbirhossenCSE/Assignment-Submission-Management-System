using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class ClassEntity : BaseEntity
{
    [BsonRequired]
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public bool IsActive { get; set; } = true;
}
