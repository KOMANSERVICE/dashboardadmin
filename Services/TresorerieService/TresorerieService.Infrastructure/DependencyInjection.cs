using IDR.Library.BuildingBlocks;
using IDR.Library.BuildingBlocks.Contexts;
using IDR.Library.BuildingBlocks.Security;
using IDR.Library.BuildingBlocks.Security.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TresorerieService.Infrastructure.Data;

namespace TresorerieService.Infrastructure;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
       (this IServiceCollection services, IConfiguration configuration)
    {
        var vaultUri = configuration["Vault:Uri"]!;
        var roleId = configuration["Vault:RoleId"]!;
        var secretId = configuration["Vault:SecretId"]!;
        var dataBase = configuration.GetConnectionString("Database")!;
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

        services.AddDbContext<TresorerieDbContext>((sp, opts) => {
            opts.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opts.UseNpgsql(connectionString);
        });

        // Enregistrer DbContext comme type de base pour injection dans les handlers
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TresorerieDbContext>());

        services.AddGenericRepositories<TresorerieDbContext>();
  
        services.AddContextMiddleware();

        //services.AddScoped<ITresorerieDbContext, TresorerieDbContext>();

        // API Key Security Services
        // services.AddScoped<IApiKeyFactory, ApiKeyFactory>();
        //services.AddApiKeyAuthentication();

        return services;
    }

    public static WebApplication UseInfrastructureServices(this WebApplication app)
    {
        app.UseContextMiddleware();
        return app;
    }
}