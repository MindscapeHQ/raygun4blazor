using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raygun.Samples.Blazor.Maui
{
    public static class MauiExtensions
    {
        public static MauiAppBuilder UseRaygunBlazorMaui(this MauiAppBuilder builder)
        {
            return builder;
        }
    }
}
