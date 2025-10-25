namespace BackendAdmin.Infrastructure.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.Items["UserId"] as string;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("User ID is missing in the current context.");

        return userId;

    }
    public string GetEmail()
    {
        return _httpContextAccessor.HttpContext?.Items["Email"] as string;
    }
}

