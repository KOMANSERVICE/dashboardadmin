

namespace MenuService.Api;

public static class DependencyInjection
{
    private static string MyAllowSpecificOrigins = "AllowOrigin";
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddCarter();

        services.AddExceptionHandler<CustomExceptionHandler>();


        services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("https://localhost:9112","https://backendadmin.api:9112","https://backendadmin.api:8081", "https://depensio.api:9102")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
        });

        services.AddEndpointsApiExplorer();

        services.AddAuthorization();
        services.AddAuthentication();
        services.AddOpenApi();
        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseHttpsRedirection();
        app.UseCors(MyAllowSpecificOrigins);
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
