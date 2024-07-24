using KristofferStrube.Blazor.Window;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Raygun.NetCore.Blazor;
using System;
using System.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Extension methods for adding Raygun components to Dependency Injection <see cref="IServiceCollection" /> containers.
    /// </summary>
    public static class Raygun_Blazor_IServiceCollectionExtensions
    {

        #region Static Fields

        /// <summary>
        /// RWM: Addresses accidental serial requests in Chrome.
        /// https://github.com/dotnet/aspnetcore/issues/26795#issuecomment-707356648
        /// </summary>
        /// <remarks>
        /// We're reusing the same instance instead of creating a new one per Client to save memory.
        /// </remarks>
        internal static readonly CacheControlHeaderValue CacheControlHeaderValue = new() { NoCache = true };

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds Raygun services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> instance to extend.</param>
        /// <param name="configuration">The IConfiguration instance to pull settings from.</param>
        /// <param name="configSectionName">
        /// The name of the section in appsettings.json to get the RaygunSettings from. Defaults to "Raygun".
        /// </param>
        /// <remarks>
        /// If you're using Blazor WebAssembly, please call builder.UseRaygunBlazor() in your Program.cs file instead.
        /// </remarks>
        public static void AddRaygunBlazor(this IServiceCollection services, IConfiguration configuration, string configSectionName = "Raygun")
        {
            services.Configure<RaygunSettings>(configuration.GetSection(configSectionName));
            services.AddScoped<RaygunBrowserInterop>();
            services.AddWindowService();

            services.AddHttpClient("Raygun")
                .ConfigureHttpClient((sp, client) =>
                {
                    var raygunSettings = sp.GetRequiredService<IOptions<RaygunSettings>>().Value;
                    client.BaseAddress = new Uri(raygunSettings.Endpoint);
                    client.DefaultRequestHeaders.Add("X-ApiKey", raygunSettings.ApiKey);
                    client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue;
                    // TODO: RWM: Set user agent
                });

            services.AddScoped<RaygunBlazorClient>();
        }

        #endregion

    }

}
