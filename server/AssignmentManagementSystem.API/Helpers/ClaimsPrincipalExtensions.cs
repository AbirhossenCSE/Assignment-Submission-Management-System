using System.Security.Claims;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }

    public static Role? GetUserRole(this ClaimsPrincipal user)
    {
        var roleString = user.FindFirstValue(ClaimTypes.Role);
        if (Enum.TryParse<Role>(roleString, out var role))
        {
            return role;
        }
        return null;
    }
}
