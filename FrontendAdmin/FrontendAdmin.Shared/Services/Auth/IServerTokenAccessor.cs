using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FrontendAdmin.Shared.Services.Auth;

public interface IServerTokenAccessor
{
    Task<(string? Token, IEnumerable<Claim> Claims)> GetTokenAsync();
    Task SetTokenAsync(string token, IEnumerable<Claim> claims);
    Task ClearTokenAsync();
}
