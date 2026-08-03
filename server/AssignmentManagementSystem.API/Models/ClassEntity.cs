namespace AssignmentManagementSystem.API.Models;

public class ClassEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public List<string> StudentIds { get; set; } = new();
}
