using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raygun.NetCore.Samples.Blazor.WebAssembly.ViewModels;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Raygun.NetCore.Samples.Blazor.WebAssembly
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.UseRaygunBlazor();
            builder.Services.AddSingleton<CounterViewModel>();

            // RWM: Remove HttpClient logging in non-development environments.
            if (!builder.HostEnvironment.IsDevelopment())
            {
                // https://stackoverflow.com/questions/63958542/how-can-i-turn-off-info-logging-in-browser-console-from-httpclients-in-blazor
                builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            }

            await builder.Build().RunAsync();
        }
    }
}
