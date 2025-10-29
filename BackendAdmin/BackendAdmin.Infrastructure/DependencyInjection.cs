using BackendAdmin.Application.Data;
using BackendAdmin.Infrastructure.Data.Interceptors;
using Microsoft.AspNetCore.Identity;

namespace BackendAdmin.Infrastructure;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructureServices
      (this IServiceCollection services, IConfiguration configuration)
    {

        var vaultUri = configuration["Vault:Uri"]!;
        var roleId = configuration["Vault:RoleId"]!;
        var secretId = configuration["Vault:SecretId"]!;
        var dataBase = configuration.GetConnectionString("DashAdminDatabase")!;

        if (string.IsNullOrEmpty(dataBase))
        {
            throw new InvalidOperationException("Database connection string is not provided in configuration");
        }

        if(string.IsNullOrEmpty(vaultUri) ||
            string.IsNullOrEmpty(roleId) ||
            string.IsNullOrEmpty(secretId))
        {
            throw new InvalidOperationException("Vault configuration is not provided in configuration");
        }

        services.AddSingleton<ISecureSecretProvider>(sp =>
            new VaultSecretProvider(
                configuration: configuration,
                vaultUri: vaultUri,
                roleId: roleId,
                secretId: secretId
            )
        );

        var tempProvider = services.BuildServiceProvider();
        var vaultSecretProvider = tempProvider.GetRequiredService<ISecureSecretProvider>();
        var connectionString = vaultSecretProvider.GetSecretAsync(dataBase).Result;

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddGenericRepositories<ApplicationDbContext>();

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        services.AddScoped<IKeyManagementService, KeyManagementService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IUserContextService, UserContextService>();

        services.AddHttpContextAccessor();

        return services;
    }
    public static WebApplication UseInfrastructureServices(this WebApplication app)
    {
        app.UseMiddleware<UserContextMiddleware>();
        return app;
    }
}
