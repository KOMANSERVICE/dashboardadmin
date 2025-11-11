namespace MenuService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
       (this IServiceCollection services, IConfiguration configuration)
    {
        //var vaultUri = configuration["Vault:Uri"]!;
        //var roleId = configuration["Vault:RoleId"]!;
        //var secretId = configuration["Vault:SecretId"]!;
        //var dataBase = configuration.GetConnectionString("DataBase")!;


        //if (string.IsNullOrEmpty(dataBase))
        //{
        //    throw new InvalidOperationException("Database connection string is not provided in configuration");
        //}

        //if (string.IsNullOrEmpty(vaultUri) ||
        //    string.IsNullOrEmpty(roleId) ||
        //    string.IsNullOrEmpty(secretId))
        //{
        //    throw new InvalidOperationException("Vault configuration is not provided in configuration");
        //}

        //services.AddSingleton<ISecureSecretProvider>(sp =>
        //    new VaultSecretProvider(
        //        configuration: configuration,
        //        vaultUri: vaultUri,
        //        roleId: roleId,
        //        secretId: secretId
        //    )
        //);

        //var tempProvider = services.BuildServiceProvider();
        //var vaultSecretProvider = tempProvider.GetRequiredService<ISecureSecretProvider>();
        //var connectionString = vaultSecretProvider.GetSecretAsync(dataBase).Result ?? "";
        
        services.AddDbContext<MenuDbContext>((sp, opts) => {
            //opts.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opts.UseSqlite("Data Source=menuservicedb");
        });

        services.AddGenericRepositories<MenuDbContext>();

        return services;
    }
}
