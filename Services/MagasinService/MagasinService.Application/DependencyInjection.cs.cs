namespace MagasinService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices
       (this IServiceCollection services, IConfiguration configuration)
    {

        services.AddValidationBehaviors(Assembly.GetExecutingAssembly());

        return services;
    }
}
