using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FrontendAdmin.Shared.Pages.Auths.Models;
using FrontendAdmin.Shared.Services.Https;

namespace FrontendAdmin.Shared.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthHttpService _authHttpService;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(
        IAuthHttpService authHttpService,
        CustomAuthStateProvider authStateProvider)
    {
        _authHttpService = authHttpService;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> SignInAsync(SignInRequest request)
    {

        var result = await _authHttpService.SignIn(request);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Data.Token))
        {
            return false;
        }
        _authStateProvider.AuthenticateUser(result.Data.Token);
        //var claims = JwtClaimParser.FromToken(result.Data.Token).ToList();
        //claims.Add(new Claim("token", result.Data.Token));
        //var claims = new List<Claim>
        //{
        //    new Claim("token", result.Data.Token)
        //};
        //var currentUser = _customAuthenticationService.CurrentUser;

        //var identity = new ClaimsIdentity(claims,"Custom Authentication");

        //var newUser = new ClaimsPrincipal(identity);
        //_customAuthenticationService.CurrentUser = newUser;
        //await _authStateProvider.SaveTokenAsync(result.Data.Token, claims);
        return true;
    }

    //public Task LogoutAsync()
    //{
    //    return _authStateProvider.ClearTokenAsync();
    //}

    //public Task<string?> GetTokenAsync() => Task.FromResult(_authStateProvider.CurrentToken);

    public Task<string?> LoadTokenAsync() => _authStateProvider.LoadTokenAsync();

    //public Task SaveTokenAsync(string token, IEnumerable<Claim> claims) =>
    //    _authStateProvider.SaveTokenAsync(token, claims);
}
