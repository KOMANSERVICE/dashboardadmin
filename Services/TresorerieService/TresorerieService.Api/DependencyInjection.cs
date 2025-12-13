

namespace TresorerieService.Api;


public static class DependencyInjection
{
    private static string MyAllowSpecificOrigins = "AllowOrigin";
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddCarter();

        services.AddExceptionHandler<CustomExceptionHandler>();


        var tempProvider = services.BuildServiceProvider();
        var vaultSecretProvider = tempProvider.GetRequiredService<ISecureSecretProvider>();

        //Add cors
        var Allow_origin = configuration["Allow:Origins"]!;
        var origin = vaultSecretProvider.GetSecretAsync(Allow_origin).Result;
        var origins = origin.Split(';', StringSplitOptions.RemoveEmptyEntries).ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins(origins)
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