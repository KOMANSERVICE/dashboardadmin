using Blazored.LocalStorage;
using Blazored.SessionStorage;
using FrontendAdmin.Shared;
using FrontendAdmin.Shared.Services;
using FrontendAdmin.Web.Client.Services;
using IDR.Library.Blazor.Cookies;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ✅ CHARGER LA CONFIG DEPUIS L'ENDPOINT /api/config AVANT TOUT
try
{
    using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

    var configResponse = await httpClient.GetFromJsonAsync<ConfigResponse>("/api/config");
    var apiUrl = configResponse?.ApiSettings?.Uri;

    Console.WriteLine($"🌐 API URL chargée depuis /api/config : {apiUrl}");

    // Remplacer la configuration par celle de l'API
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ApiSettings:Uri"] = apiUrl
    });
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erreur lors du chargement de /api/config : {ex.Message}");
    // Fallback sur la config par défaut
}

// Add device-specific services used by the FrontendAdmin.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>()
                .AddScoped<IStorageService, SecureStorageService>()
                .AddBlazoredLocalStorage()                
                .AddBlazoredSessionStorage()
                .AddAuthorizationCore()
                .AddSharedServices(builder.Configuration);



await builder.Build().RunAsync();

public class ConfigResponse
{
    public ApiSettingsConfig? ApiSettings { get; set; }
}

public class ApiSettingsConfig
{
    public string? Uri { get; set; }
}

