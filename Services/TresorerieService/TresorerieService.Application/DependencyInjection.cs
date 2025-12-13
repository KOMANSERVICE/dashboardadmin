
namespace TresorerieService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices
        (this IServiceCollection services)
    {

        services.AddValidationBehaviors(Assembly.GetExecutingAssembly());

        return services;
    }
}
