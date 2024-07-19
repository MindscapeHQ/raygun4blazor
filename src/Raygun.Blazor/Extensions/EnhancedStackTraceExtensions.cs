using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Raygun.Blazor.Extensions
{

    /// <summary>
    /// Extension methods for the <see cref="EnhancedStackTrace" /> class.
    /// </summary>
    internal static class RaygunBlazorEnhancedStackTraceExtensions
    {

        /// <summary>
        /// Filters an <see cref="EnhancedStackTrace" />for <see cref="StackFrame">StackFrames</see> that DO NOT include types from this assembly.
        /// </summary>
        /// <param name="stackTrace">The <see cref="EnhancedStackTrace" /> instance to extend.</param>
        /// <returns>
        /// <see cref="StackFrame">StackFrames</see> that DO NOT include types from this assembly.
        /// </returns>
        internal static IEnumerable<StackFrame> GetExternalFrames(this EnhancedStackTrace stackTrace)
        {
            // RWM: Contrast this with the classname-based comparison un the Raygun4NetCore implementation that will
            //      incorrectly exclude classes if someone else creates classes in their own code that match our class names,
            //      OR changes the namespace of the Raygun4NetCore classes.
            return stackTrace.GetFrames()
                .Where(c => c.GetMethod()?.DeclaringType?.Assembly.FullName != typeof(RaygunBlazorEnhancedStackTraceExtensions).Assembly.FullName);
        }

    }

}
