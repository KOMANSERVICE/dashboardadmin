
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var database = builder.Configuration.GetConnectionString("Database");

builder.Services.AddDbContext<MenuContext>((sp,opts) =>{
    //opts.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
    opts.UseSqlite(database);
    });


//builder.Services.AddInterceptors();
builder.Services.AddGenericRepositories<MenuContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}

// Configure the HTTP request pipeline.
//app.MapGrpcService<GreeterService>();
app.MapGrpcService<MenuService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
