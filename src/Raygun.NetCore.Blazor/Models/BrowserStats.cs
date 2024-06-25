namespace Raygun.NetCore.Blazor.Models
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
        /// 
        /// </summary>
        public int AppHeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AppWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long CalculatedStorageRemainingInBytes => StorageQuotaInBytes - StorageUsageInBytes;

        /// <summary>
        /// 
        /// </summary>
        public decimal DevicePixelRatio { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long MemoryCurrentSizeInBytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long MemoryMaxSizeInBytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long MemoryUsedSizeInBytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string NetworkEffectiveType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BrowserOrientationType Orientation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long StorageQuotaInBytes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long StorageUsageInBytes { get; set; }

        #endregion

    }

}
