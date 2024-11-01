using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Raygun.Blazor.Models;

namespace Raygun.Blazor.Maui
{
    public static class MauiExtensions
    {
        public static MauiAppBuilder UseRaygunBlazorMaui(this MauiAppBuilder builder, string configSectionName = "Raygun")
        {
            #if ANDROID
            // Replace default AssemblyReaderProvider with the Android Assembly reader from Raygun4Maui
            ErrorDetails.AssemblyReaderProvider = AndroidUtilities.CreateAssemblyReader()!.TryGetReader;
            #endif
            
            builder.Services.Configure<RaygunSettings>(builder.Configuration.GetSection(configSectionName));
            builder.Services.AddScoped<RaygunBrowserInterop>();
            builder.Services.AddScoped<IWindowService, WindowService>();

            builder.Services.AddHttpClient("Raygun")
                .ConfigureHttpClient((sp, client) =>
                {
                    var raygunSettings = sp.GetRequiredService<IOptions<RaygunSettings>>().Value;
                    client.BaseAddress = new Uri(raygunSettings.Endpoint);
                    client.DefaultRequestHeaders.Add("X-ApiKey", raygunSettings.ApiKey);
                });

            builder.Services.AddScoped<RaygunBlazorClient>();
            return builder;
        }
    }
}
