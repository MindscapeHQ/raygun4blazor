using KristofferStrube.Blazor.Window;
using Raygun.NetCore.Blazor;
using System;
using System.Threading.Tasks;

namespace Raygun.NetCore.Samples.Blazor.WebAssembly.ViewModels
{

    /// <summary>
    /// 
    /// </summary>
    public class CounterViewModel
    {

        private readonly RaygunBlazorClient _raygunClient;
        private readonly IWindowService _windowService;

        /// <summary>
        /// 
        /// </summary>
        public int CurrentCount { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Action StateHasChanged { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raygunClient"></param>
        public CounterViewModel(RaygunBlazorClient raygunClient, IWindowService windowService)
        {
            _raygunClient = raygunClient;
            _windowService = windowService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task IncrementCountAsync()
        {
            CurrentCount++;
            if (CurrentCount % 3 == 0)
            {
                CurrentCount /= 0;
            }
            StateHasChanged?.Invoke();
        }

        public async Task ThrowException()
        {
            var window = await _windowService.GetWindowAsync();
            await window.PostMessageAsync("causeError");
        }

    }

}
