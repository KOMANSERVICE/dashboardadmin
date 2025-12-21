using BackendAdmin.Application.Services;
using IDR.Library.BuildingBlocks.Behaviors;

namespace BackendAdmin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices
       (this IServiceCollection services, IConfiguration configuration)
    {

        services.AddValidationBehaviors(Assembly.GetExecutingAssembly());

        services.AddFeatureManagement();
        //services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());

        services.AddScoped<AuthServices>();

        services.AddSingleton<IDockerSwarmService, DockerSwarmService>();

        return services;
    }

}
