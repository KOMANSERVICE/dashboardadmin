using FrontendAdmin.Shared;
using FrontendAdmin.Shared.Services;
using FrontendAdmin.Shared.Services.Auth;
using FrontendAdmin.Web.Components;
using FrontendAdmin.Web.Services;
using FrontendAdmin.Web.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var keysPath = Path.Combine(builder.Environment.ContentRootPath, "Keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("FrontendAdmin.Web");

builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<IServerTokenAccessor, HttpContextTokenAccessor>();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSharedServices(builder.Configuration);
// Add device-specific services used by the FrontendAdmin.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>()
                .AddScoped<IStorageService, WebSecureStorageService>();

builder.Services.AddScoped<ProtectedLocalStorage>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(FrontendAdmin.Shared._Imports).Assembly);

app.Run();
