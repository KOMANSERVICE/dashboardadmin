using FrontendAdmin.Shared.Services;
using FrontendAdmin.Shared;
using FrontendAdmin.Web.Components;
using FrontendAdmin.Web.Services;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the FrontendAdmin.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>()
                .AddScoped<IStorageService, WebSecureStorageService>()
                .AddScoped<ProtectedLocalStorage>()
                .AddScoped<ProtectedSessionStorage>()
                .AddSharedServices(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/config", (IConfiguration config) =>
{
    var apiUrl = Environment.GetEnvironmentVariable("API_SETTINGS_URL")
                 ?? config["ApiSettings:Uri"];

    return Results.Json(new { ApiSettings = new { Uri = apiUrl } });
});

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(FrontendAdmin.Shared._Imports).Assembly,
        typeof(FrontendAdmin.Web.Client._Imports).Assembly)
    .WithStaticAssets();

app.Run();
