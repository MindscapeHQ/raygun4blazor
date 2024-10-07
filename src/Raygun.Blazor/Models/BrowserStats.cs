namespace Raygun.Blazor.Models
{

    /// <summary>
    /// Attributes about the Browser that are highly likely to change at runtime.
    /// </summary>
    /// <remarks>
    /// This exists because we need to parse some stuff from the browser to construct a proper <see cref="EnvironmentDetails" />
    /// instance. Also the <see cref="EnvironmentDetails" /> has serialization needs for Raygun that don't align with JSInterop.
    /// </remarks>
    internal record BrowserStats
    {

        #region Public Properties

        /// <summary>
        /// Application height. From `screen.availHeight`.
        /// </summary>
        public int AppHeight { get; set; }

        /// <summary>
        /// Application width. From `screen.availWidth`.
        /// </summary>
        public int AppWidth { get; set; }

        /// <summary>
        /// Returns the calculated storage remaining in bytes.
        /// </summary>
        public long CalculatedStorageRemainingInBytes => StorageQuotaInBytes - StorageUsageInBytes;

        /// <summary>
        /// Device pixel ratio. From `window.devicePixelRatio`.
        /// </summary>
        public decimal DevicePixelRatio { get; set; }

        /// <summary>
        /// JavaScript heap memory size in bytes. From `window.performance.memory.totalJSHeapSize`.
        /// </summary>
        public long MemoryCurrentSizeInBytes { get; set; }

        /// <summary>
        /// Max JS heap memory size in bytes. From `window.performance.memory.jsHeapSizeLimit`.
        /// </summary>
        public long MemoryMaxSizeInBytes { get; set; }

        /// <summary>
        /// Used JS heap memory size in bytes. From `window.performance.memory.userJSHeapSize`.
        /// </summary>
        public long MemoryUsedSizeInBytes { get; set; }

        /// <summary>
        /// Network type.
        /// </summary>
        /// <remarks>
        /// Currently not set.
        /// </remarks>
        public string? NetworkEffectiveType { get; set; }

        /// <summary>
        /// Orientation type. From `screen.orientation.type`.
        /// </summary>
        public BrowserOrientationType Orientation { get; set; }

        /// <summary>
        /// Storage quota in bytes. From `navigator.storage.estimate().quota`.
        /// </summary>
        public long StorageQuotaInBytes { get; set; }

        /// <summary>
        /// Storage usage in bytes. From `navigator.storage.estimate().usage`.
        /// </summary>
        public long StorageUsageInBytes { get; set; }

        #endregion

    }

}
