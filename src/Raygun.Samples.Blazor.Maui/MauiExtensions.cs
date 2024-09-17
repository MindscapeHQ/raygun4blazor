using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using Raygun.Blazor;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Offline;
using Raygun.Blazor.Offline.SendStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TODO: Move to package
namespace Raygun.Samples.Blazor.Maui
{
    public static class MauiExtensions
    {
        public static MauiAppBuilder UseRaygunBlazorMaui(this MauiAppBuilder builder, string configSectionName = "Raygun")
        {
            builder.Services.Configure<RaygunSettings>(builder.Configuration.GetSection(configSectionName));
            builder.Services.AddScoped<RaygunBrowserInterop>();
            builder.Services.AddScoped<IWindowService, WindowService>();
            builder.Services.AddScoped<IBackgroundSendStrategy, TimerBasedSendStrategy>();
            builder.Services.AddScoped<IRaygunOfflineStore, RaygunLocalOfflineStore>();

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
