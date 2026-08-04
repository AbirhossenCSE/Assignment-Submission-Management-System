using AssignmentManagementSystem.API.Models;

namespace AssignmentManagementSystem.API.Helpers.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
