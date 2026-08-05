using AssignmentManagementSystem.API.Common.Enums;
using AssignmentManagementSystem.API.Helpers.Interfaces;

namespace AssignmentManagementSystem.API.Helpers.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User.GetUserId() ?? string.Empty;

    public string Email => _httpContextAccessor.HttpContext?.User.GetUserEmail() ?? string.Empty;

    public Role? Role => _httpContextAccessor.HttpContext?.User.GetUserRole();
}
