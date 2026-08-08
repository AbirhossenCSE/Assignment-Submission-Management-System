using AssignmentManagementSystem.API.Common.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace AssignmentManagementSystem.API.Models;

public class User : BaseEntity
{
    [BsonRequired]
    public string FullName { get; set; } = string.Empty;

    [BsonRequired]
    public string Email { get; set; } = string.Empty;

    [BsonRequired]
    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }

    public string? ClassId { get; set; }

    public bool IsActive { get; set; } = true;
}
