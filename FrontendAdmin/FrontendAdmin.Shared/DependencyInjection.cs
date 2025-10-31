using IDR.Library.Blazor.Auths;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;
        Console.WriteLine($"API URI: {uri}");

        services.AddAuthServices();

        services.AddRefitClient<IAuthHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri));

        services.AddRefitClient<IAppAdminHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri))
            .AddHttpMessageHandler<JwtAuthorizationHandler>();

        return services;
    }
}
