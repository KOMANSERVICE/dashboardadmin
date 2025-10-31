using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace FrontendAdmin.Shared.Services.Auth;

public class HttpContextTokenAccessor : IServerTokenAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTokenAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<(string? Token, IEnumerable<Claim> Claims)> GetTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null || context.User.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult<(string?, IEnumerable<Claim>)>((null, Array.Empty<Claim>()));
        }

        var tokenClaim = context.User.FindFirst("access_token");
        var token = tokenClaim?.Value;
        var claims = context.User.Claims;
        return Task.FromResult<(string?, IEnumerable<Claim>)>((token, claims));
    }

    public async Task SetTokenAsync(string token, IEnumerable<Claim> claims)
    {
        var context = _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HTTP context available");

        var claimList = new List<Claim>(claims)
        {
            new Claim("access_token", token)
        };

        var identity = new ClaimsIdentity(claimList, "jwt");
        var principal = new ClaimsPrincipal(identity);

        await context.SignInAsync("FrontendAdminAuth", principal, new AuthenticationProperties
        {
            IsPersistent = true,
            AllowRefresh = true
        });

        context.User = principal;
    }

    public async Task ClearTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        await context.SignOutAsync("FrontendAdminAuth");
    }
}
