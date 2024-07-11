using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raygun.NetCore.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.WebAssembly.Hosting
{

    /// <summary>
    /// Extensions for registering RaygunBlazorClient and related services with a Blazor WebAssembly application.
    /// </summary>
    public static class Raygun_WebAssembly_WebAssemblyHostBuilderExtensions
    {

        /// <summary>
        /// Configures the RaygunBlazorClient and related services for use in a Blazor WebAssembly application.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="WebAssemblyHostBuilder" /> instance to extend.
        /// </param>
        /// <param name="configSectionName">
        /// The name of the section in appsettings.json to get the RaygunSettings from. Defaults to "Raygun".
        /// </param>
        /// <remarks>
        /// Unlike a Blazor Web app, Raygun will inject Singleton instances of the required services into the DI container.
        /// This is because WebAssembly runs in the local browser, so we will not be needing multiple instances of the same
        /// service.
        /// </remarks>
        public static void UseRaygunBlazor(this WebAssemblyHostBuilder builder, string configSectionName = "Raygun")
        {
            builder.Services.Configure<RaygunSettings>(builder.Configuration.GetSection(configSectionName));
            builder.Services.AddSingleton<RaygunBrowserInterop>();
            builder.Services.AddSingleton<IWindowService, WindowService>();

            builder.Services.AddHttpClient("Raygun")
                .ConfigureHttpClient((sp, client) =>
                {
                    var raygunSettings = sp.GetRequiredService<IOptions<RaygunSettings>>().Value;
                    client.BaseAddress = new Uri(raygunSettings.Endpoint);
                    client.DefaultRequestHeaders.Add("X-ApiKey", raygunSettings.ApiKey);
                    client.DefaultRequestHeaders.CacheControl = Raygun_Blazor_IServiceCollectionExtensions.CacheControlHeaderValue;
                    // TODO: RWM: Set user agent
                });

            builder.Services.AddSingleton<RaygunBlazorClient>();

        }

    }

}
