using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using IDR.Library.BuildingBlocks.Security.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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



        var JWT_Secret = configuration["JWT:Secret"]!;
        var JWT_ValidIssuer = configuration["JWT:ValidIssuer"]!;
        var JWT_ValidAudience = configuration["JWT:ValidAudience"]!;

        var secret = vaultSecretProvider.GetSecretAsync(JWT_Secret).Result;
        var issuer = vaultSecretProvider.GetSecretAsync(JWT_ValidIssuer).Result;
        var audience = vaultSecretProvider.GetSecretAsync(JWT_ValidAudience).Result;


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
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero //Supprime la tolérance de 5 min par défaut
                };
            });

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
        //services.AddOpenApi("public");

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

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Add the security scheme at the document level
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Description = "Authorization oauth2",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            // Apply it as a requirement for all operations
            foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }
        }
    }
}