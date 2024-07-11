# Raygun for Blazor

## Usage

### Installation

- [ ] TODO: Package installation instructions. e.g.

```
dotnet add package <something>
```

### Setup

#### Using `WebAssemblyHostBuilder` to create the client

Use `UseRaygunBlazor` extension to configure the `RaygunBlazorClient` and related services for use in a Blazor WebAssembly application.

```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseRaygunBlazor();
```

The `RaygunSettings` will be obtained from the `appsettings.json` under the configuration section name `Raygun`.

For all configuration values, check the `RaygunSettings` class under `src/Raygun.NetCore.Blazor/RaygunSettings.cs`.

#### Initalize the client

Inject the `RaygunBlazorClient` in your code:

```cs
@inject RaygunBlazorClient raygunClient;
```

And call to `raygunClient.InitializeAsync()` at least once.

- [ ] TODO: Add more info. See `src/Raygun.NetCore.Blazor/RaygunBlazorClient.cs` for more information.

### Recording an error

Call to `raygunClient.RecordExceptionAsync(...)`

- [ ] TODO: Add more info. See `src/Raygun.NetCore.Blazor/RaygunBlazorClient.cs` for more information.

### Recording a breadcrumb

Call to `raygunClient.RecordBreadcrumb(...);`

- [ ] TODO: Add more info. See `src/Raygun.NetCore.Blazor/RaygunBlazorClient.cs` for more information.

### `RaygunErrorBoundary`

- [ ] TODO: Document when to use the `RaygunErrorBoundary`.

Currently used in `src/Raygun.NetCore.Samples.Blazor.WebAssembly/App.razor`

## Example Project

Example project is located in `src/Raygun.NetCore.Samples.Blazor.WebAssembly`

To run the example:

1. Install `dotnet-sdk` minimum version supported in `8.0.300`.
2. Add the `ApiKey` property to in `src/Raygun.NetCore.Samples.Blazor.WebAssembly/wwwroot/appsettings.json`

```
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

3. Run `dotnet watch` from the example folder.

A browser window to `http://localhost:5010/` should automatically open.

## Publishing

- [ ] TODO: Packagre publishing instructions, e.g. NuGet publish instructions
