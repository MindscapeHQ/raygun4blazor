using System;
using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Raygun.Blazor.Server.Extensions
{

    /// <summary>
    /// 
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configSectionName"></param>
        public static void UseRaygunBlazor(this WebApplicationBuilder builder, string configSectionName = "Raygun")
        {
            builder.Services.Configure<RaygunSettings>(builder.Configuration.GetSection(configSectionName));
            builder.Services.AddScoped<RaygunBrowserInterop>();
            builder.Services.AddScoped<IWindowService, WindowService>();

            builder.Services.AddHttpClient("Raygun")
                .ConfigureHttpClient((sp, client) =>
                {
                    var raygunSettings = sp.GetRequiredService<IOptions<RaygunSettings>>().Value;
                    client.BaseAddress = new Uri(raygunSettings.Endpoint);
                    client.DefaultRequestHeaders.Add("X-ApiKey", raygunSettings.ApiKey);
                    // TODO: RWM: Set user agent
                });

            builder.Services.AddScoped<RaygunBlazorClient>();
        }
    }

}
