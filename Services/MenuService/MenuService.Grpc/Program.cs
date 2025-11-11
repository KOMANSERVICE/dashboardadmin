using MenuService.Application;
using MenuService.Grpc.Services;
using MenuService.Infrastructure;
using MenuService.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

builder.Services.AddGrpc();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

app.MapGrpcService<MenuGrpcService>();

app.Run();
