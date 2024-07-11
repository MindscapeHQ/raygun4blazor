using System;

namespace Raygun.NetCore.Blazor
{

    /// <summary>
    /// 
    /// </summary>
    internal static class NamingUtilities
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Blazor creates weird type and method names for code that is compiled from inside the Razor file itself.
        /// In many cases, the async code method name will be "MoveNext", which is incredibly unhelpful. When that happens,
        /// We can grab the actual method name out of the generated type name.
        /// </remarks>
        internal static ReadOnlySpan<char> GetBlazorMethodName(string methodName)
        {
            int start = methodName.IndexOf('<') + 1;
            int end = methodName.IndexOf('>', start);
            return start > 0 && end > start ? methodName[start..end].AsSpan() : null;
        }




    }
}
