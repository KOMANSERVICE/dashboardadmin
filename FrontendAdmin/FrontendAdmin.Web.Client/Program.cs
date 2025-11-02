using Blazored.LocalStorage;
using Blazored.SessionStorage;
using FrontendAdmin.Shared;
using FrontendAdmin.Shared.Services;
using FrontendAdmin.Web.Client.Services;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


// Add device-specific services used by the FrontendAdmin.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>()
                .AddScoped<IStorageService, SecureStorageService>()
                .AddBlazoredLocalStorage()                
                .AddBlazoredSessionStorage()
                .AddAuthorizationCore()
                .AddSharedServices(builder.Configuration);

await builder.Build().RunAsync();
