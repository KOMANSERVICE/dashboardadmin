using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FrontendAdmin.Shared.Pages.Auths.Models;

namespace FrontendAdmin.Shared.Services.Auth;

public interface IAuthService
{
    Task<bool> SignInAsync(SignInRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task LoadTokenAsync();
}
