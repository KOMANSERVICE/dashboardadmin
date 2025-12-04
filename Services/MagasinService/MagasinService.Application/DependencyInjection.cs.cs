namespace MagasinService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices
       (this IServiceCollection services, IConfiguration configuration)
    {
        // Ajouter MediatR (inclus dans IDR.Library.BuildingBlocks)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidationBehaviors(Assembly.GetExecutingAssembly());

        return services;
    }
}
