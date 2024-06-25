var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
window.raygunBlazor = {
    _raygunInterop: null,
    /**
     *
     */
    initialize: function (raygunInterop) {
        this._raygunInterop = raygunInterop;
    },
    /**
     *
     */
    recordBreadcrumb: function (message, category, level, customData) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this._raygunInterop.invokeMethodAsync('RecordJsBreadcrumb', message, category, level, customData);
        });
    },
    /**
     *
     */
    recordException: function (exception, tags, sendUserData, customData) {
        return __awaiter(this, void 0, void 0, function* () {
            yield this._raygunInterop.invokeMethodAsync('RecordJsException', exception, tags, sendUserData, customData);
        });
    },
};
/**
 * A function called by the RaygunBlazorClient's JavaScript interop to get details about the browser that don't
 * usually change during the user's session. It will be called once at startup and the results will be cached in
 * .NET.
 */
export function getBrowserSpecs() {
    return __awaiter(this, void 0, void 0, function* () {
        var specs = {
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
            specs.DeviceMemoryInGB = navigator.deviceMemory;
        }
        // RWM: The "isExtended" property is not available in Firefox or Safari.
        //      See: https://developer.mozilla.org/en-US/docs/Web/API/Screen/isExtended#browser_compatibility
        if ('isExtended' in screen) {
            specs.HasMultipleMonitors = screen.isExtended;
        }
        // RWM: The "userAgentData" API is not available in Firefox or Safari.
        //      See: https://developer.mozilla.org/en-US/docs/Web/API/Navigator/userAgentData#browser_compatibility
        if ('userAgentData' in navigator) {
            var ua = yield navigator.userAgentData.getHighEntropyValues([
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
            ua.brands.map((item) => {
                specs.UAHints.BrandVersions[item.brand] = item.version;
            });
            ua.fullVersionList.map((item) => {
                specs.UAHints.ComponentVersions[item.brand] = item.version;
            });
        }
        return specs;
    });
}
/**
 * A function called by the RaygunBlazorClient's JavaScript interop to get details about the browser that change
 * frequently during the user's session. It will be called every time a new exception is recorded.
 */
export function getBrowserStats() {
    return __awaiter(this, void 0, void 0, function* () {
        var stats = {
            AppHeight: screen.availHeight,
            AppWidth: screen.availWidth,
            DevicePixelRatio: window.devicePixelRatio,
            Orientation: screen.orientation.type,
        };
        // RWM: The "memory" API is not available in Firefox or Safari.
        //      See: https://developer.mozilla.org/en-US/docs/Web/API/Performance/memory#browser_compatibility
        if ('memory' in window.performance) {
            var memory = window.performance.memory;
            stats.MemoryCurrentSizeInBytes = memory.totalJSHeapSize;
            stats.MemoryMaxSizeInBytes = memory.jsHeapSizeLimit;
            stats.MemoryUsedSizeInBytes = memory.usedJSHeapSize;
        }
        // RWM: This can throw an exception is storage is disabled.
        //      See: https://developer.mozilla.org/en-US/docs/Web/API/StorageManager/estimate#exceptions
        try {
            var storage = yield navigator.storage.estimate();
            stats.StorageQuotaInBytes = storage.quota;
            stats.StorageUsageInBytes = storage.usage;
            //environment.diskSpaceFree = [((storage.quota - storage.usage) / 1024 / 1024 / 1024).toFixed(4)];
        }
        catch (_a) { }
        return stats;
    });
}
