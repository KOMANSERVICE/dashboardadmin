using FrontendAdmin.Services;
using FrontendAdmin.Services.Auth;
using FrontendAdmin.Shared;
using FrontendAdmin.Shared.Services;
using FrontendAdmin.Shared.Services.Auth;
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

            builder.Services.AddSingleton<IServerTokenAccessor, MauiTokenAccessor>();
            builder.Services.AddSharedServices(builder.Configuration);
            // Add device-specific services used by the FrontendAdmin.Shared project
            builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

