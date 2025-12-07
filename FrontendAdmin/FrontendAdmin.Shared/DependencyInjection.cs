using IDR.Library.Blazor.Toasts;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;

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

        services.AddScoped<ToastService>();

        return services;
    }
}
