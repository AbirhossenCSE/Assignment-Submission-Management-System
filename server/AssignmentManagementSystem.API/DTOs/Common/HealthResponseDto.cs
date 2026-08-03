namespace AssignmentManagementSystem.API.DTOs.Common;

public class HealthResponseDto
{
    public string Status { get; set; } = "Healthy";
    public string Message { get; set; } = "API is running";
    public DatabaseStatusDto Database { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DatabaseStatusDto
{
    public string Status { get; set; } = "Disconnected";
    public string Details { get; set; } = string.Empty;
}
