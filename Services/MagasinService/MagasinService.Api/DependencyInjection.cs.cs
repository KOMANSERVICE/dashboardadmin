using Carter;
using IDR.Library.BuildingBlocks.Exceptions.Handler;

namespace MagasinService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddCarter();

        services.AddExceptionHandler<CustomExceptionHandler>();

        services.AddOpenApi();
        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHttpsRedirection();

        return app;
    }
}