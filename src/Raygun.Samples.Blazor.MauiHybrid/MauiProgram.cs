using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raygun.Blazor;
using Raygun.Blazor.Maui.Extensions;
using System.Reflection;

namespace Raygun.Samples.Blazor.MauiHybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Add Raygun configuration
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("appsettings.json");

            // MAUI App should be configured using Raygun4MAUI
            var builder = MauiApp.CreateBuilder();
            builder.Configuration.AddJsonStream(stream!);
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .UseRaygunBlazor();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
