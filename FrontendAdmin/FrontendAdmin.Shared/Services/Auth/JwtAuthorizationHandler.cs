using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FrontendAdmin.Shared.Services.Auth;

public class JwtAuthorizationHandler(
    CircuitServicesAccessor circuitServicesAccessor,
    IAuthService _authService
    ) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //var token = await _authService.GetTokenAsync();

        //if (string.IsNullOrWhiteSpace(token))
        //{
        //    token = await _authService.LoadTokenAsync();
        //}

        //if (!string.IsNullOrWhiteSpace(token))
        //{
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //}

        var authStateProvider = circuitServicesAccessor.Services?
           .GetRequiredService<AuthenticationStateProvider>();

        if (authStateProvider is null)
        {
            throw new Exception("AuthenticationStateProvider not available");
        }

        var authState = await authStateProvider.GetAuthenticationStateAsync();

        var user = authState?.User;
        if (user?.Identity is not null && user.Identity.IsAuthenticated)
        {
            var token = user.FindFirst("token")?.Value;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
