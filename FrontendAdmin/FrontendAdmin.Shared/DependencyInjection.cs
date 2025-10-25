using FrontendAdmin.Shared.Services.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace FrontendAdmin.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uri = configuration["ApiSettings:Uri"]!;

        services.AddRefitClient<IAuthHttpService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(uri));

        return services;
    }
}
