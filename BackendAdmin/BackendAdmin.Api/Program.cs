using BackendAdmin.Api;
using BackendAdmin.Application;
using BackendAdmin.Infrastructure;
using BackendAdmin.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddEnvironmentVariables();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiServices()
    .UseInfrastructureServices();

if (app.Environment.IsDevelopment())
{

    await app.InitialiseDatabaseAsync();
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.Run();

