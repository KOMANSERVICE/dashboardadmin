using IDR.Library.Blazor.Toasts;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;

        
        if (string.IsNullOrWhiteSpace(uri))
        {
            throw new InvalidOperationException(
                "ApiSettings:Uri is not configured. Please check your appsettings.json file.");
        }

        services.AddBlazorLibrairyServices(configuration, (options) =>
        {
            options.Uri = uri;
            options.Logout = "logout";
            options.PageTitle = "ADMIN DASHBOARD - ";
        });

        services.AddRefitClient<IAuthHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>();

        services.AddRefitClient<IAppAdminHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddRefitClient<IMenuHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddRefitClient<IApiKeyHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        // Tresorerie Service
        var tresorerieUri = configuration["ApiSettings:TresorerieUri"] ?? uri;
        services.AddRefitClient<ITresorerieHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(tresorerieUri))
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        services.AddScoped<ToastService>();

        return services;
    }
}
