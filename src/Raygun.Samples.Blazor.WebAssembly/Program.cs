using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Raygun.Blazor.Interfaces;
using Raygun.Blazor.Models;
using Raygun.Blazor.WebAssembly.Extensions;
using Raygun.Samples.Blazor.WebAssembly.ViewModels;

namespace Raygun.Samples.Blazor.WebAssembly
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient
            { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Provide a custom implementation of IRaygunUserManager

            // Simple IRaygunUserManager
            // builder.Services.AddSingleton<IRaygunUserManager, MyUserManager>();

            // IRaygunUserManager with AuthenticationStateProvider
            builder.Services.AddAuthorizationCore();
            builder.Services.AddSingleton<AuthenticationStateProvider, TestAuthStateProvider>();
            builder.Services.AddSingleton<IRaygunUserManager, RaygunUserManagerAuthState>();

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

    /// <summary>
    /// Custom IRaygunUserManager that providers user details directly
    /// </summary>
    public class MyUserManager : IRaygunUserManager
    {
        public Task<UserDetails?> GetCurrentUser()
        {
            return Task.FromResult(new UserDetails()
            {
                FullName = "Test User from MyUserManager",
                Email = "test-user-manager@example.com",
                UserId = "67890",
            })!;
        }
    }

    /// <summary>
    /// Test AuthenticationStateProvider that returns a user
    /// </summary>
    public class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Test user from TestAuthStateProvider"),
                new(ClaimTypes.Email, "test-auth-user@example.com"),
            };
            var anonymous = new ClaimsIdentity(claims, "testAuthType");
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
        }
    }

    /// <summary>
    /// Custom IRaygunManager that uses AuthenticationStateProvider to get user information
    /// </summary>
    public class RaygunUserManagerAuthState(AuthenticationStateProvider authenticationStateProvider)
        : IRaygunUserManager
    {
        public async Task<UserDetails?> GetCurrentUser()
        {
            var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var details = new UserDetails()
            {
                FullName = user.FindFirst(ClaimTypes.Name)?.Value,
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
            };
            return details;
        }
    }
}