
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontendAdmin.Shared.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{

    private readonly IServerTokenAccessor _tokenAccessor;

    //private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    //private ClaimsPrincipal _currentUser;
    //private string? _currentToken;
    //private bool _isInitialized;

    //public CustomAuthStateProvider(IServerTokenAccessor tokenAccessor)
    //{
    //    _tokenAccessor = tokenAccessor;
    //    _currentUser = _anonymous;
    //}

    //public override Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    return Task.FromResult(new AuthenticationState(_isInitialized ? _currentUser : _anonymous));
    //}

    public async Task<string?> LoadTokenAsync()
    {
        var (token, claims) = await _tokenAccessor.GetTokenAsync();
        this.AuthenticateUser(token);
        return token;
    }

    //public async Task SaveTokenAsync(string token, IEnumerable<Claim> claims)
    //{
    //    await _tokenAccessor.SetTokenAsync(token, claims);
    //    ApplyAuthenticatedState(token, claims);
    //}

    //public async Task ClearTokenAsync()
    //{
    //    await _tokenAccessor.ClearTokenAsync();
    //    SetAnonymous();
    //}

    //public string? CurrentToken => _currentToken;
    //public bool IsInitialized => _isInitialized;

    //private void ApplyAuthenticatedState(string token, IEnumerable<Claim> claims)
    //{
    //    var claimList = claims as IList<Claim> ?? claims.ToList();
    //    if (claimList.Count == 0)
    //    {
    //        claimList = JwtClaimParser.FromToken(token).ToList();
    //    }

    //    _currentToken = token;
    //    _currentUser = new ClaimsPrincipal(new ClaimsIdentity(claimList, "jwt"));
    //    _isInitialized = true;
    //    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    //}

    //private void SetAnonymous()
    //{
    //    _currentToken = null;
    //    _currentUser = _anonymous;
    //    _isInitialized = true;
    //    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    //}
    private AuthenticationState authenticationState;

    private readonly CustomAuthenticationService _customAuthenticationService;
    public CustomAuthStateProvider(CustomAuthenticationService service,
        IServerTokenAccessor tokenAccessor)
    {
        authenticationState = new AuthenticationState(service.CurrentUser);

        service.UserChanged += (newUser) =>
        {
            authenticationState = new AuthenticationState(newUser);
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        };
        _customAuthenticationService = service;
        _tokenAccessor = tokenAccessor;
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(authenticationState);

    public void AuthenticateUser(string token)
    {
        var claims = new List<Claim>
        {
            new Claim("token", token)
        };
        var currentUser = _customAuthenticationService.CurrentUser;

        var identity = new ClaimsIdentity(claims, "Custom Authentication");

        var newUser = new ClaimsPrincipal(identity);
        _customAuthenticationService.CurrentUser = newUser;
        _tokenAccessor.SetTokenAsync(token, claims);

    }
}
