(<any>window).raygunBlazor = {
    _raygunInterop: null,
    _errorListener: null,
    _rejectionListener: null,

    /**
     * Wires the .NET interop reference and attaches null-safe global error
     * listeners. Replaces the old KristofferStrube.Blazor.Window dependency,
     * which crashed on iOS/Safari when ErrorEvent.error was null.
     */
    initialize: function(raygunInterop: any) {
        this._raygunInterop = raygunInterop;

        if (this._errorListener === null) {
            this._errorListener = (event: ErrorEvent) => {
                const payload = (<any>window).raygunBlazor._buildErrorPayload(event);
                if (payload === null) {
                    return;
                }
                (<any>window).raygunBlazor._raygunInterop.invokeMethodAsync(
                    'RecordJsException',
                    payload,
                    ['UnhandledException', 'Blazor', 'JavaScript'],
                    null,
                ).catch(() => { /* swallow: never let Raygun itself surface a JS error. */ });
            };
            window.addEventListener('error', this._errorListener);
        }

        if (this._rejectionListener === null) {
            this._rejectionListener = (event: PromiseRejectionEvent) => {
                const payload = (<any>window).raygunBlazor._buildRejectionPayload(event);
                if (payload === null) {
                    return;
                }
                (<any>window).raygunBlazor._raygunInterop.invokeMethodAsync(
                    'RecordJsException',
                    payload,
                    ['UnhandledRejection', 'Blazor', 'JavaScript'],
                    null,
                ).catch(() => { /* swallow */ });
            };
            window.addEventListener('unhandledrejection', this._rejectionListener);
        }
    },

    /**
     * Builds a serializable payload from an ErrorEvent, defending against the
     * common case (iOS/Safari, cross-origin scripts) where event.error is null.
     */
    _buildErrorPayload: function(event: ErrorEvent): any {
        const err: any = event && (<any>event).error ? (<any>event).error : null;
        const message = (err && typeof err.message === 'string' && err.message)
            || (event && (<any>event).message)
            || '';
        const name = (err && typeof err.name === 'string' && err.name) || 'Error';

        if (!message && !err) {
            // Cross-origin "Script error." with no useful data — skip.
            return null;
        }

        return {
            Name: name,
            Message: message,
            Stack: (err && typeof err.stack === 'string') ? err.stack : '',
            FileName: (event && (<any>event).filename) || '',
            LineNumber: (event && typeof (<any>event).lineno === 'number') ? (<any>event).lineno : 0,
            ColumnNumber: (event && typeof (<any>event).colno === 'number') ? (<any>event).colno : 0,
        };
    },

    /**
     * Builds a serializable payload from a PromiseRejectionEvent.
     */
    _buildRejectionPayload: function(event: PromiseRejectionEvent): any {
        const reason: any = event ? (<any>event).reason : null;

        if (reason instanceof Error) {
            return {
                Name: typeof reason.name === 'string' ? reason.name : 'UnhandledRejection',
                Message: typeof reason.message === 'string' ? reason.message : '',
                Stack: typeof reason.stack === 'string' ? reason.stack : '',
                FileName: '',
                LineNumber: 0,
                ColumnNumber: 0,
            };
        }

        if (reason === null || reason === undefined) {
            return null;
        }

        let message: string;
        try {
            message = typeof reason === 'string' ? reason : JSON.stringify(reason);
        } catch {
            message = String(reason);
        }

        return {
            Name: 'UnhandledRejection',
            Message: message,
            Stack: '',
            FileName: '',
            LineNumber: 0,
            ColumnNumber: 0,
        };
    },

    /**
     *
     */
    recordBreadcrumb: async function(message: string, type: string, category: string, level: string, customData: Record<string, string>) {
        await this._raygunInterop.invokeMethodAsync('RecordJsBreadcrumb', message, type, category, level, customData);
    },

    /**
     *
     */
    recordException: async function(exception: Error, tags: string[], customData: Record<string, string>) {
        const payload = {
            Name: (exception && typeof exception.name === 'string') ? exception.name : 'Error',
            Message: (exception && typeof exception.message === 'string') ? exception.message : '',
            Stack: (exception && typeof exception.stack === 'string') ? exception.stack : '',
            FileName: '',
            LineNumber: 0,
            ColumnNumber: 0,
        };
        await this._raygunInterop.invokeMethodAsync('RecordJsException', payload, tags, customData);
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
 * Represents the JS version of the BrowserSpecs class in the Raygun.Blazor project. It is designed to store details
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
 * Represents the JS version of the BrowserStats class in the Raygun.Blazor project. It is designed to store details
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
 * Represents the JS version of the BrowserUserAgentData class in the Raygun.Blazor project. It is designed to store
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
