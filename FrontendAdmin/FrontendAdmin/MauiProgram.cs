using FrontendAdmin.Services;
using FrontendAdmin.Shared;
using FrontendAdmin.Shared.Services;
using IDR.Library.Blazor.LocalStorages;
using Microsoft.Extensions.Logging;

namespace FrontendAdmin
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // Add device-specific services used by the FrontendAdmin.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>()
                .AddScoped<IStorageService, MauiSecureStorageService>()
                .AddSharedServices(builder.Configuration);

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddAuthorizationCore();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
