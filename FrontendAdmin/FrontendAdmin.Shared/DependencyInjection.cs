using System;
using System.Linq;
using FrontendAdmin.Shared.Services.Auth;
using FrontendAdmin.Shared.Services.Https;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;

        //if (!services.Any(d => d.ServiceType == typeof(IServerTokenAccessor)))
        //{
        //    throw new InvalidOperationException("IServerTokenAccessor must be registered by the host application before calling AddSharedServices.");
        //}

        //services.AddAuthorizationCore();
        services.AddScoped<CircuitServicesAccessor>();
        services.AddScoped<CustomAuthenticationService>();
        services.AddScoped<JwtAuthorizationHandler>();
        services.AddScoped<CustomAuthStateProvider>();
        services.AddScoped<IAuthService, AuthService>();
        //services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
        services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

        services.AddRefitClient<IAuthHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri));

        services.AddRefitClient<IAppAdminHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        return services;
    }
}
