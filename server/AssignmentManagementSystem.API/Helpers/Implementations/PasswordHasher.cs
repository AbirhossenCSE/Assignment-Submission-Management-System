using AssignmentManagementSystem.API.Helpers.Interfaces;
using BCrypt.Net;

namespace AssignmentManagementSystem.API.Helpers.Implementations;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
