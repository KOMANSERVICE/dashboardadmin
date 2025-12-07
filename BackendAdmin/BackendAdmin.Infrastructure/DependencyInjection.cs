using BackendAdmin.Application.ApiExterne.Menus;
using BackendAdmin.Application.Data;
using BackendAdmin.Application.Services;
using IDR.Library.BuildingBlocks.Contexts;
using IDR.Library.BuildingBlocks.Interceptors;
using Microsoft.AspNetCore.Identity;
using Refit;

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
        var pathMountPoint = configuration["Vault:PathMountPoint"]!;
        var mountPoint = configuration["Vault:MountPoint"]!;
        var menuServiceUri = configuration["Service:MenuUrl"]!;

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

        if(string.IsNullOrEmpty(pathMountPoint))
        {
            throw new InvalidOperationException("Vault path mount point is not provided in configuration");
        }

        if (string.IsNullOrEmpty(mountPoint))
        {
            throw new InvalidOperationException("Vault mount point is not provided in configuration");
        }

        if (string.IsNullOrEmpty(menuServiceUri))
        {
            throw new InvalidOperationException("Menu service URI is not provided in configuration");
        }

        services.AddSingleton<ISecureSecretProvider>(sp =>
            new VaultSecretProvider(
                vaultUri: vaultUri,
                roleId: roleId,
                secretId: secretId,
                pathMountPoint: pathMountPoint,
                mountPoint: mountPoint
            )
        );

        var tempProvider = services.BuildServiceProvider();
        var vaultSecretProvider = tempProvider.GetRequiredService<ISecureSecretProvider>();
        var connectionString = vaultSecretProvider.GetSecretAsync(dataBase).Result;
        var menu_url = vaultSecretProvider.GetSecretAsync(menuServiceUri).Result ?? "";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.Configure<IdentityOptions>(options =>
        {
            // Default Password settings.
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddGenericRepositories<ApplicationDbContext>();
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        services.AddSecurities();
        services.AddContextMiddleware();

        // Register ApiKey services
        services.AddScoped<IApiKeyService, ApiKeyService>();

        services.AddRefitClient<IMenuService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(menu_url));

        return services;
    }
    public static WebApplication UseInfrastructureServices(this WebApplication app)
    {
        app.UseContextMiddleware();
        return app;
    }
}
