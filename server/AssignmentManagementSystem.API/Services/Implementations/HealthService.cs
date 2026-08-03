using AssignmentManagementSystem.API.DTOs.Common;
using AssignmentManagementSystem.API.Services.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Services.Implementations;

public class HealthService : IHealthService
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<HealthService> _logger;

    public HealthService(IMongoDatabase database, ILogger<HealthService> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<HealthResponseDto> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = new HealthResponseDto
        {
            Status = "Healthy",
            Message = "API is running",
            Timestamp = DateTime.UtcNow
        };

        try
        {
            using var pingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            pingCts.CancelAfter(TimeSpan.FromSeconds(2));

            var command = new BsonDocument("ping", 1);
            await _database.RunCommandAsync<BsonDocument>(command, cancellationToken: pingCts.Token);
            
            response.Database.Status = "Connected";
            response.Database.Details = $"Connected to MongoDB database '{_database.DatabaseNamespace.DatabaseName}' successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning("MongoDB health check ping failed or timed out: {Message}", ex.Message);
            response.Database.Status = "Disconnected";
            response.Database.Details = $"MongoDB connection unavailable: {ex.Message}";
        }

        return response;
    }
}
