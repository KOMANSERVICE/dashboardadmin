using IDR.Library.Blazor.Auths;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;

        services.AddAuthServices(configuration, (options) =>
        {
            options.Uri = uri;
            options.Logout = "logout";
        });

        services.AddRefitClient<IAuthHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>();

        services.AddRefitClient<IAppAdminHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<CookieHandler>()
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        return services;
    }
}
