
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace FrontendAdmin.Shared.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IStorageService _storage;
    private const string TokenKey = "authToken";
    private bool _isInitialized = false;
    private ClaimsPrincipal? _cachedUser;
    private AuthenticationState authenticationState;
    private readonly CustomAuthenticationService _customAuthenticationService;
    public CustomAuthStateProvider(CustomAuthenticationService service, IStorageService storage)
    {
        authenticationState = new AuthenticationState(service.CurrentUser);

        service.UserChanged += (newUser) =>
        {
            authenticationState = new AuthenticationState(newUser);
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        };
        _storage = storage;
        _customAuthenticationService = service;
    }

    private ClaimsPrincipal Anonymous => new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(authenticationState);

    public async Task LoadTokenAsync()
    {
        try
        {
            var token = await _storage.GetAsync(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                var claims = ParseClaimsFromJwt(token);
                _cachedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
            }
        }
        catch
        {
            // En cas d'erreur (ex: rendu statique), on reste anonyme
            _cachedUser = Anonymous;
        }
        finally
        {
            _isInitialized = true;
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var claims = new List<Claim>
                {
                    new Claim("token", token)
                };
        var currentUser = _customAuthenticationService.CurrentUser;

        var identity = new ClaimsIdentity(claims, "Custom Authentication");

        var newUser = new ClaimsPrincipal(identity);
        _customAuthenticationService.CurrentUser = newUser;
        //_tokenAccessor.SetTokenAsync(token, claims);
    }

    public void NotifyUserLogout()
    {
        _cachedUser = Anonymous;
        _isInitialized = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = DecodeBase64Url(payload);
            var claimsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)!;

            var claims = new List<Claim>();

            foreach (var kvp in claimsDict)
            {
                if (kvp.Key.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                    kvp.Key.Equals("roles", StringComparison.OrdinalIgnoreCase))
                {
                    if (kvp.Value is JsonElement element)
                    {
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in element.EnumerateArray())
                            {
                                claims.Add(new Claim(ClaimTypes.Role, item.GetString() ?? string.Empty));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, element.GetString() ?? string.Empty));
                        }
                    }
                    continue;
                }

                var claimType = MapJwtClaimType(kvp.Key);
                claims.Add(new Claim(claimType, kvp.Value?.ToString() ?? string.Empty));
            }

            claims.Add(new Claim("token", jwt));
            return claims;
        }
        catch
        {
            // En cas d'erreur de parsing JWT, retourner des claims vides
            return Enumerable.Empty<Claim>();
        }
    }

    private static string MapJwtClaimType(string key)
    {
        return key switch
        {
            "name" or "unique_name" or "nameid" or "sub" => ClaimTypes.Name,
            "email" => ClaimTypes.Email,
            "given_name" => ClaimTypes.GivenName,
            "family_name" => ClaimTypes.Surname,
            "sid" => ClaimTypes.Sid,
            _ => key
        };
    }

    private static byte[] DecodeBase64Url(string base64Url)
    {
        var s = base64Url.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}

//public class CustomAuthStateProvider : AuthenticationStateProvider
//{

//    private readonly IServerTokenAccessor _tokenAccessor;

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

    //public async Task<string?> LoadTokenAsync()
    //{
    //    var (token, claims) = await _tokenAccessor.GetTokenAsync();
    //    this.AuthenticateUser(token);
    //    return token;
    //}

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
//    private AuthenticationState authenticationState;

//    private readonly CustomAuthenticationService _customAuthenticationService;
//    public CustomAuthStateProvider(CustomAuthenticationService service,
//        IServerTokenAccessor tokenAccessor)
//    {
//        authenticationState = new AuthenticationState(service.CurrentUser);

//        service.UserChanged += (newUser) =>
//        {
//            authenticationState = new AuthenticationState(newUser);
//            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
//        };
//        _customAuthenticationService = service;
//        _tokenAccessor = tokenAccessor;
//    }
//    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
//        Task.FromResult(authenticationState);

//    public void AuthenticateUser(string token)
//    {
//        var claims = new List<Claim>
//        {
//            new Claim("token", token)
//        };
//        var currentUser = _customAuthenticationService.CurrentUser;

//        var identity = new ClaimsIdentity(claims, "Custom Authentication");

//        var newUser = new ClaimsPrincipal(identity);
//        _customAuthenticationService.CurrentUser = newUser;
//        _tokenAccessor.SetTokenAsync(token, claims);

//    }
//}
