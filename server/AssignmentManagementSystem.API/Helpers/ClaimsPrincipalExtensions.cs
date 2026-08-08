using System.Security.Claims;
using AssignmentManagementSystem.API.Common.Enums;

namespace AssignmentManagementSystem.API.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
        return idClaim?.Value ?? string.Empty;
    }

    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        var emailClaim = user.FindFirst(ClaimTypes.Email);
        return emailClaim?.Value ?? string.Empty;
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        var nameClaim = user.FindFirst(ClaimTypes.Name);
        return nameClaim?.Value ?? string.Empty;
    }

    public static Role? GetUserRole(this ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && Enum.TryParse<Role>(roleClaim.Value, out var role))
        {
            return role;
        }
        return null;
    }

    public static string? GetClassId(this ClaimsPrincipal user)
    {
        var classIdClaim = user.FindFirst("classId");
        return classIdClaim?.Value;
    }
}
