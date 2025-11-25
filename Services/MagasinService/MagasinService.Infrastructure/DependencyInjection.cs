
namespace MagasinService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
       (this IServiceCollection services, IConfiguration configuration)
    {
        var vaultUri = configuration["Vault:Uri"]!;
        var roleId = configuration["Vault:RoleId"]!;
        var secretId = configuration["Vault:SecretId"]!;
        var dataBase = configuration.GetConnectionString("MenuDatabase")!;
        var pathMountPoint = configuration["Vault:PathMountPoint"]!;
        var mountPoint = configuration["Vault:MountPoint"]!;


        if (string.IsNullOrEmpty(dataBase))
        {
            throw new InvalidOperationException("Database connection string is not provided in configuration");
        }

        if (string.IsNullOrEmpty(vaultUri) ||
            string.IsNullOrEmpty(roleId) ||
            string.IsNullOrEmpty(secretId))
        {
            throw new InvalidOperationException("Vault configuration is not provided in configuration");
        }

        if (string.IsNullOrEmpty(pathMountPoint))
        {
            throw new InvalidOperationException("Vault path mount point is not provided in configuration");
        }

        if (string.IsNullOrEmpty(mountPoint))
        {
            throw new InvalidOperationException("Vault mount point is not provided in configuration");
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
        var connectionString = vaultSecretProvider.GetSecretAsync(dataBase).Result ?? "";

        services.AddDbContext<MagasinServiceDbContext>((sp, opts) => {
            //opts.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opts.UseNpgsql(connectionString);
        });


        services.AddGenericRepositories<MagasinServiceDbContext>();

        services.AddScoped<IMagasinServiceDbContext, MagasinServiceDbContext>();

        return services;
    }
}
