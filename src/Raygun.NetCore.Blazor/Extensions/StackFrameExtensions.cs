using Raygun.NetCore.Blazor;

namespace System.Diagnostics
{

    /// <summary>
    /// Extension methods for the <see cref="StackFrame" /> class.
    /// </summary>
    internal static class Raygun_Blazor_StackFrameExtensions
    {

        /// <summary>
        /// Returns consistent class and method names regardless of how the classes were generated / compiled.
        /// </summary>
        /// <param name="stackFrame">The <see cref="StackFrame" /> instance to extend.</param>
        /// <returns>
        /// A tuple of strings containing the Class Name and Method Name.
        /// </returns>
        /// <remarks>
        /// RWM: This exists because the Razor-based stack traces will contain names like 
        /// "Raygun.NetCore.Samples.Blazor.WebAssembly.ViewModels.CounterViewModel+<IncrementCountAsync>d__10" which contains
        /// both the class and method names in the type name, and the reported method name will be "MoveNext" for async methods.
        /// Obviously this is totally unhelpful for developers, so we need to make it more magical.
        /// </remarks>
        public static (string ClassName, string MethodName) GetBlazorNames(this StackFrame stackFrame)
        {
            var method = stackFrame.GetMethod();
            var declaringTypeName = method.DeclaringType.FullName;
            var isBlazorType = declaringTypeName.Contains("+<");

            return isBlazorType ?
                // RWM: If we're in a .razor file, the real type name will be one level up on the inheritance chain,
                //      while the real method name will be embedded in the current type's name. Fun!
                (method.DeclaringType.DeclaringType.FullName, NamingUtilities.GetBlazorMethodName(declaringTypeName).ToString()) :
                // RWM: Otherwise, we're in a normal class, so we can just use the type and method names as-is.
                (declaringTypeName, method.Name);
        }

    }

}
