(<any>window).raygunBlazor = {
    _raygunInterop: null,

    /**
     * 
     */
    initialize: function(raygunInterop: any) {
        this._raygunInterop = raygunInterop;
    },

    /**
     * 
     */
    recordBreadcrumb: async function(message: string, category: string, level: string, customData: Record<string, string>) {
        await this._raygunInterop.invokeMethodAsync('RecordJsBreadcrumb', message, category, level, customData);
    },

    /**
     * 
     */
    recordException: async function(exception: DOMException, tags: string[], sendUserData: boolean, customData: Record<string, string>) {
        await this._raygunInterop.invokeMethodAsync('RecordJsException', exception, tags, sendUserData, customData);
    },
};

/**
 * A function called by the RaygunBlazorClient's JavaScript interop to get details about the browser that don't 
 * usually change during the user's session. It will be called once at startup and the results will be cached in
 * .NET.
 */
export async function getBrowserSpecs(): Promise<BrowserSpecs> {
    var specs: BrowserSpecs = {
        AreCookiesEnabled: navigator.cookieEnabled,
        ColorDepth: screen.colorDepth,
        Locale: navigator.language,
        PixelDepth: screen.pixelDepth,
        Platform: navigator.platform,
        ProcessorCount: navigator.hardwareConcurrency,
        ScreenHeight: screen.height,
        ScreenWidth: screen.width,
        UserAgent: navigator.userAgent,
        UtcOffset: new Date().getTimezoneOffset() / -60,
    };

    // RWM: The "deviceMemory" property is not available in Firefox or Safari.
    //      See: https://developer.mozilla.org/en-US/docs/Web/API/Navigator/deviceMemory#browser_compatibility
    if ('deviceMemory' in navigator) {
        specs.DeviceMemoryInGB = (<any>navigator).deviceMemory;
    }

    // RWM: The "isExtended" property is not available in Firefox or Safari.
    //      See: https://developer.mozilla.org/en-US/docs/Web/API/Screen/isExtended#browser_compatibility
    if ('isExtended' in screen) {
        specs.HasMultipleMonitors = (<any>screen).isExtended;
    }

    // RWM: The "userAgentData" API is not available in Firefox or Safari.
    //      See: https://developer.mozilla.org/en-US/docs/Web/API/Navigator/userAgentData#browser_compatibility
    if ('userAgentData' in navigator) {
        var ua = await (<any>navigator.userAgentData).getHighEntropyValues([
            "architecture",
            "bitness",
            "formFactor",
            "fullVersionList",
            "model",
            "platform",
            "platformVersion",
            "wow64"
        ]);
        specs.UAHints = {
            Architecture: ua.architecture,
            Bitness: ua.bitness,
            BrandVersions: {},
            ComponentVersions: {},
            FormFactor: ua.formFactor,
            IsMobile: ua.mobile,
            IsWow64: ua.wow64,
            Model: ua.model,
            Platform: ua.platform,
            PlatformVersion: ua.platformVersion
        };
        ua.brands.map((item: { brand: string, version: string }) => {
            specs.UAHints.BrandVersions[item.brand] = item.version;
        });
        ua.fullVersionList.map((item: { brand: string, version: string }) => {
            specs.UAHints.ComponentVersions[item.brand] = item.version;
        });
    }
    return specs;
}

/**
 * A function called by the RaygunBlazorClient's JavaScript interop to get details about the browser that change 
 * frequently during the user's session. It will be called every time a new exception is recorded.
 */
export async function getBrowserStats(): Promise<BrowserStats> {
    var stats: BrowserStats = {
        AppHeight: screen.availHeight,
        AppWidth: screen.availWidth,
        DevicePixelRatio: window.devicePixelRatio,
        Orientation: screen.orientation.type,
    };

    // RWM: The "memory" API is not available in Firefox or Safari.
    //      See: https://developer.mozilla.org/en-US/docs/Web/API/Performance/memory#browser_compatibility
    if ('memory' in window.performance) {
        var memory = (<any>window.performance.memory);
        stats.MemoryCurrentSizeInBytes = memory.totalJSHeapSize;
        stats.MemoryMaxSizeInBytes = memory.jsHeapSizeLimit;
        stats.MemoryUsedSizeInBytes = memory.usedJSHeapSize;
    }

    // RWM: This can throw an exception is storage is disabled.
    //      See: https://developer.mozilla.org/en-US/docs/Web/API/StorageManager/estimate#exceptions
    try {
        var storage = await navigator.storage.estimate();
        stats.StorageQuotaInBytes = storage.quota;
        stats.StorageUsageInBytes = storage.usage;
        //environment.diskSpaceFree = [((storage.quota - storage.usage) / 1024 / 1024 / 1024).toFixed(4)];
    }
    catch {}
    return stats;
}

/**
 * Represents the JS version of the BrowserSpecs class in the Raygun.NetCore.Blazor project. It is designed to store details
 * about the browser that don't usually change during the user's session.
 */
export interface BrowserSpecs {
    AreCookiesEnabled: boolean;
    ColorDepth: number;
    DeviceMemoryInGB?: number;
    HasMultipleMonitors?: boolean;
    Locale: string;
    PixelDepth: number;
    Platform: string;
    ProcessorCount: number;
    ScreenHeight: number;
    ScreenWidth: number;
    UAHints?: BrowserUserAgentData;
    UserAgent: string;
    UtcOffset: number;
}

/**
 * Represents the JS version of the BrowserStats class in the Raygun.NetCore.Blazor project. It is designed to store details
 * about the browser that change frequently during the user's session.
 */
export interface BrowserStats {
    AppHeight: number;
    AppWidth: number;
    DevicePixelRatio: number;
    Orientation: string;
    MemoryCurrentSizeInBytes?: number;
    MemoryMaxSizeInBytes?: number;
    MemoryUsedSizeInBytes?: number;
    StorageQuotaInBytes?: number;
    StorageUsageInBytes?: number;
}

/**
 * Represents the JS version of the BrowserUserAgentData class in the Raygun.NetCore.Blazor project. It is designed to store
 * the output of the navigator.userAgentData.getHighEntropyValues() method.
 */
export interface BrowserUserAgentData {
    Architecture?: string;
    Bitness?: string;
    BrandVersions: Record<string, string>;
    ComponentVersions?: Record<string, string>;
    FormFactor?: string
    IsMobile?: boolean;
    IsWow64?: boolean
    Model?: string;
    Platform?: string;
    PlatformVersion?: string;
}