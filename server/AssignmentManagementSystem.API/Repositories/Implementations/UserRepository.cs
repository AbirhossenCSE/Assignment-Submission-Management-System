using System.Collections.Concurrent;
using AssignmentManagementSystem.API.Models;
using AssignmentManagementSystem.API.Repositories.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AssignmentManagementSystem.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User>? _usersCollection;
    private static readonly ConcurrentDictionary<string, User> _inMemoryUsers = new();
    private readonly ILogger<UserRepository> _logger;
    private readonly bool _isMongoAvailable;

    public UserRepository(IMongoDatabase database, ILogger<UserRepository> logger)
    {
        _logger = logger;
        try
        {
            _usersCollection = database.GetCollection<User>("Users");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            database.RunCommand<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token);
            _isMongoAvailable = true;
        }
        catch
        {
            _isMongoAvailable = false;
            _logger.LogWarning("MongoDB connection unavailable. Operating in resilient dev mode.");
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (_isMongoAvailable && _usersCollection != null)
        {
            try
            {
                return await _usersCollection
                    .Find(u => u.Email == normalizedEmail)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store.");
            }
        }

        var user = _inMemoryUsers.Values.FirstOrDefault(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));
        return await Task.FromResult(user);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _usersCollection != null)
        {
            try
            {
                return await _usersCollection
                    .Find(u => u.Id == id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store.");
            }
        }

        _inMemoryUsers.TryGetValue(id, out var user);
        return await Task.FromResult(user);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.Email = user.Email.Trim().ToLowerInvariant();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = ObjectId.GenerateNewId().ToString();
        }

        if (_isMongoAvailable && _usersCollection != null)
        {
            try
            {
                await _usersCollection.InsertOneAsync(user, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB insert failed, saving in memory.");
            }
        }

        _inMemoryUsers[user.Id] = user;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_isMongoAvailable && _usersCollection != null)
        {
            try
            {
                return await _usersCollection.Find(_ => true).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB query failed, using in-memory store.");
            }
        }

        return await Task.FromResult<IEnumerable<User>>(_inMemoryUsers.Values);
    }

    public async Task UpdateAsync(string id, User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        if (_isMongoAvailable && _usersCollection != null)
        {
            try
            {
                await _usersCollection.ReplaceOneAsync(u => u.Id == id, user, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB update failed, updating in memory.");
            }
        }

        _inMemoryUsers[id] = user;
    }
}
