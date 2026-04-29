using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Raygun.Blazor;

namespace Raygun.Samples.Blazor.WebAssembly.ViewModels
{

    /// <summary>
    ///
    /// </summary>
    public class CounterViewModel
    {

        private readonly RaygunBlazorClient _raygunClient;
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        ///
        /// </summary>
        public int CurrentCount { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public Action? StateHasChanged { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="raygunClient"></param>
        /// <param name="jsRuntime"></param>
        public CounterViewModel(RaygunBlazorClient raygunClient, IJSRuntime jsRuntime)
        {
            _raygunClient = raygunClient;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Task IncrementCountAsync()
        {
            CurrentCount++;
            if (CurrentCount % 3 == 0)
            {
                CurrentCount /= 0;
            }
            StateHasChanged?.Invoke();
            return Task.CompletedTask;
        }

        public async Task ThrowException()
        {
            await _jsRuntime.InvokeVoidAsync("postMessage", "causeError", "*");
        }

        public async Task SendCustomJsException()
        {
            await _jsRuntime.InvokeVoidAsync("postMessage", "recordException", "*");
        }

        public async Task SendCustomJsBreadcrumb()
        {
            await _jsRuntime.InvokeVoidAsync("postMessage", "recordBreadcrumb", "*");
        }
    }
}
