using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raygun.NetCore.Blazor.WebAssembly.Controls
{

    /// <summary>
    /// An extension of the Blazor <see cref="ErrorBoundary" /> control that automatically sends exceptions to Raygun.
    /// </summary>
    public class RaygunErrorBoundary : ErrorBoundary
    {

        #region Internal Parameters

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        internal IJSRuntime JSRuntime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        internal RaygunBlazorClient RaygunClient { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        internal IOptions<RaygunSettings> RaygunSettings { get; set; }

        #endregion

        #region Public Parameters

        /// <summary>
        /// 
        /// </summary>
        [Parameter]
        public bool ShowExceptionUI { get; set; }

        #endregion

        #region Internal Methods

        /// <summary>
        /// When an error occurs, send it to Raygun.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected override async Task OnErrorAsync(Exception exception)
        {
            if (!RaygunSettings.Value.CatchUnhandledExceptions) return;

            await RaygunClient.RecordExceptionAsync(exception, ["UnhandledException", "Blazor", ".NET"]);
        }

        /// <inheritdoc />
        /// <remarks>
        /// We are rendering differently than the ErrorBoundary base control because Raygun's ethos is to first not
        /// mess with anything about the app. So if the developer wants to display UI, they have to specifically opt-in.
        /// </remarks>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (CurrentException is not null && ShowExceptionUI)
            {
                if (ErrorContent is not null)
                {
                     builder.AddContent(1, ErrorContent(CurrentException));
                }
                else
                {
                    // RWM: We may need to consider invoking JavaScript to set the "blazor-error-ui" visible instead.
                    //      The code sets the style to display-block;


                    // The default error UI doesn't include any content, because:
                    // [1] We don't know whether or not you'd be happy to show the stack trace. It depends both on
                    //     whether DetailedErrors is enabled and whether you're in production, because even on WebAssembly
                    //     you likely don't want to put technical data like that in the UI for end users. A reasonable way
                    //     to toggle this is via something like "#if DEBUG" but that can only be done in user code.
                    // [2] We can't have any other human-readable content by default, because it would need to be valid
                    //     for all languages.
                    // Instead, the default project template provides locale-specific default content via CSS. This provides
                    // a quick form of customization even without having to subclass this component.
                    builder.OpenElement(2, "div");
                    builder.AddAttribute(3, "class", "blazor-error-boundary");
                    builder.CloseElement();
                }
            }
            else
            {
                builder.AddContent(0, ChildContent);
            }
        }

        #endregion

    }

}
