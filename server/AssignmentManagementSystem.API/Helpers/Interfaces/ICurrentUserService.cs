using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.Helpers.Interfaces;

public interface ICurrentUserService
{
    string UserId { get; }
    string Email { get; }
    Role? Role { get; }
}
