# Raygun for Blazor

## Usage

### Installation

#### NuGet Package manager
The best way to install Raygun is to use the NuGet package manager. Right-click on your project and select "**Manage NuGet Packages....**". Navigate to the Browse tab, then use the search box to find **Raygun.Blazor** and install it.

#### .NET Cli

To install the latest version:

``` 
dotnet add package Raygun.Blazor
```

Alternatively, you can specify a version tag to install a specific version of the package. See [Raygun.Blazor NuGet Gallery page](https://nuget.org/packages/Raygun.Blazor) for information on available versions.

```
dotnet add package Raygun.Blazor --version x.y.z
```

### Setup

See the section **Blazor WebAssembly**, **Blazor Server** and **MAUI Blazor Hybrid** for specific setup instructions depending on the project type.

#### Raygun Settings

Raygun for Blazor uses the `appsettings.json` to load configuration settings.

`RaygunSettings` will be obtained from the configuration section name `Raygun`.

For example:

```json
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY",
    "CatchUnhandledExceptions": "false",
    "LogLevel": "Debug"
  }
}
```

For all configuration values, check the `RaygunSettings` class under `src/Raygun.Blazor/RaygunSettings.cs`.

See [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0) to learn more about managing application settings.

#### Initalize the client

Inject the `RaygunBlazorClient` in your code:

```cs
@inject RaygunBlazorClient raygunClient;
```

And call to `raygunClient.InitializeAsync()` at least once.

This method should be called as early as possible in the course of using the app. The best place to do it is inside the
`OnAfterRenderAsync` method of the main layout page. However it will also be called automatically before sending any
exceptions, just in case.

### Recording an error

To send an exception to Raygun call to `raygunClient.RecordExceptionAsync(...)`

This method accepts the following arguments:

- `ex`: The `Exception` to send back to Raygun.
- `userDetails`: Optional. Attach user details to exception, takes priority over `IRaygunUserProvider`.
- `tags`: Optional. User-specified tags that should be applied to the error.
- `userCustomData`: Optional. Any custom data that you you like sent with the report to assist with troubleshooting.
- `cancellationToken`: Optional. A `CancellationToken` to allow you to cancel the current request, if necessary.

### Recording a breadcrumb

Records a Breadcrumb to help you track what was going on in your application before an error occurred.

Call to `raygunClient.RecordBreadcrumb(...);`

This method accepts the following arguments:

- `message`: The message you want to record for this Breadcrumb.
- `type`: The `BreadcrumbType` for the message. Defaults to `BreadcrumbType.Manual`.
- `category`: A custom value used to arbitrarily group this Breadcrumb.
- `customData`: Any custom data you want to record about application state when the Breadcrumb was recorded.

### Attaching user details

Raygun for Blazor provides two ways to attach user details to error reports:

1. Provide `UserDetails` in the `RecordExceptionAsync` method call.
2. Implement a `IRaygunUserProvider`.

#### User details class

The following properties can be provided as user details:

- `UserId`: Unique identifier for the user is the user identifier.
- `IsAnonymous`: Flag indicating if the user is anonymous or not.
- `Email`: User's email address.
- `FullName`: User's full name.
- `FirstName`: User's first name (what you would use if you were emailing them - "Hi {{firstName}}, ...")
- `DeviceId`: Device unique identifier. Useful if sending errors from a mobile device.

All properties are strings except isAnonymous, which is a boolean. As well, they are all optional.

#### User details in `RecordExceptionAsync`

The simplest way to attach user details to an error report, is to do it when calling to `RecordExceptionAsync`.

Pass a `UserDetails` object to the method call:

```cs
var userDetails = new UserDetails() { Email = "test@example.com", FullName = "Test User", UserId = "123456" };

await RaygunClient.RecordExceptionAsync(ex, userDetails);
```

#### Implementing `IRaygunUserProvider`

Providing an instance of `IRaygunUserProvider` to the Raygun Blazor client allows you to attach user details also to errors reported automatically, for example, captured unhandled exceptions or exceptions from the JavaScript layer.

Implement an `IRaygunUserProvider`, for example:

```cs
public class MyUserProvider : IRaygunUserProvider
{
    public Task<UserDetails?> GetCurrentUser()
    {
        return Task.FromResult(new UserDetails());
    }
}
```

And inject it into the Raygun Blazor client:

```cs
builder.Services.AddSingleton<IRaygunUserProvider, MyUserProvider>();
```

For a complete example on how to implement a `IRaygunUserProvider` with the `AuthenticationStateProvider` check the example project file `src/Raygun.Samples.Blazor.WebAssembly/Program.cs`.

### Background processing

You can enable a throttled background message processor for the Raygun provider by setting the `UseBackgroundQueue` to `true`.

```json
{
  "Raygun": {
    "UseBackgroundQueue": true
  }
}
```

**Background processing is disabled by default.**

You can adjust the following background processor settings as well:

- `BackgroundMessageQueueMax` to set the maximum queue size.
- `BackgroundMessageWorkerCount` to set the maximum number of workers.
- `BackgroundMessageWorkerBreakpoint` to set a threshold on when a new worker will be added.

Check the `RaygunSettings` class for a complete documentation of these parameters and their default values.

### Modifying or cancelling requests

The RaygunClient exposes the `OnBeforeSend` event listener, which allows you to modify or cancel error message requests before they are sent to Raygun.

To cancel sending a request, set the `Cancel` property to `false` inside your listener:

```cs
RaygunClient.OnBeforeSend += (sender, args) =>
{
  // Example of how to cancel sending a message to Raygun
  if (args.Request.Details.Error.Message == "Cancel me")
  {
    // If the error message is "Cancel me"
    // then cancel the send
    args.Cancel = true;
  }
}
```

As well, you can modify the `Request` payload:

```cs
RaygunClient.OnBeforeSend += (sender, args) =>
{
  args.Request.Details.Error.Message = "Changed message";
};
```

### Offline storage configuration

Raygun for Blazor supports storing error reports when they can't be sent, for example, if the communication between the server and Raygun is interrupted.

You can enable offline storage support by setting the `UseOfflineStore` to `true`.

```json
{
  "Raygun": {
    "UseOfflineStore": true
  }
}
```

**Offline storage is disabled by default.**

You can adjust the following offline store settings as well:

- `DirectoryName` to set the folder to store crash reports, relative to the system local application data.
- `MaxOfflineFiles` to set the maximum number of stored reports, defaults to 50.

For example:

```json
{
  "Raygun": {
    "UseOfflineStore": true,
    "DirectoryName": "MyApp",
    "MaxOfflineFiles": 100
  }
}
```

With this configuration, Raygun will store a maximum of 100 Raygun offline reports under `C:\Users\<USER>\AppData\Local\MyApp`.

### Internal logger

Raygun for Blazor uses an internal logger to help facilitate the integration of the package.
The default log level is `"Warning"`.
To completely disable the internal logger, set the `"LogLevel"` setting to `"None"`.

For example:

```json
{
  "Raygun": {
    "LogLevel": "None"
  }
}
```

For all configuration values, check the `RaygunLogLevel` enum under `src/Raygun.Blazor/Logging/RaygunLogLevel.cs`.

---

## Blazor WebAssembly

### Installation

Install the package `Raygun.Blazor.WebAssembly` from NuGet.

### Setup

- [ ] TODO: setup WebAssembly instructions

### Example

Example project is located in `src/Raygun.Samples.Blazor.WebAssembly`

To run the example:

1. Install `dotnet-sdk` minimum version supported in `8.0.300`.
2. Add the `ApiKey` property to in `src/Raygun.Samples.Blazor.WebAssembly/wwwroot/appsettings.json`

```
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

3. Run `dotnet watch` from the example folder.

A browser window to `http://localhost:5010/` should automatically open.

## Blazor Server

### Installation

Install the package `Raygun.Blazor.Server` from NuGet.

### Setup

Add a scoped `RaygunBlazorClient` by calling to `UseRaygunBlazor()` with your `WebApplication` builder.

```cs
var builder = WebApplication.CreateBuilder(args);

...

builder.UseRaygunBlazor();
```

### Accessing `RaygunBlazorClient`

You can access the `RaygunBlazorClient` using `@inject` in your code:

```cs
@inject RaygunBlazorClient RaygunClient

...

RaygunClient.RecordExceptionAsync(...)
```

### Capturing unhandled exceptions

Use `RaygunErrorBoundary` to wrap compoments and capture unhandled exceptions automatically.

Note: You have to set `@rendermode="InteractiveServer"` in your `HeadOutlet` and `Routes` component to enable error capturing, as explained in [Handle errors in ASP.NET Core Blazor apps](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/handle-errors?view=aspnetcore-8.0#error-boundaries)

For example, in your `MainLayout.razor`:

```cs
@using Raygun.Blazor.Server.Controls

...

<article class="content px-4">
  <RaygunErrorBoundary>
    @Body
  </RaygunErrorBoundary>
</article>
```

You can set `ShowExceptionsUI="true` to display a custom error message:

```cs
<RaygunErrorBoundary ShowExceptionUI="true">
  <ChildContent>
    @Body
  </ChildContent>
  <ErrorContent>
    <p class="errorUI">👾 Error captured by Raygun!</p>
  </ErrorContent>
</RaygunErrorBoundary>
```

### Example

Example project is located in `src/Raygun.Samples.Blazor.Server`

To run the example:

1. Install `dotnet-sdk` minimum version supported in `8.0.300`.
2. Add the `ApiKey` property to in `src/Raygun.Samples.Blazor.Server/appsettings.Development.json`

```
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

3. Run `dotnet watch` from the example folder.

A browser window to `http://localhost:5010/` should automatically open.

## MAUI Blazor Hybrid

As .Net MAUI Blazor Hybrid applications are composed of both MAUI and Blazor, you will have to setup Raygun for MAUI and Raygun for Blazor separately.

Check the package [Raygun4Maui](https://www.nuget.org/packages/Raygun4Maui/) for the MAUI setup instructions.

### Installation

Install the package `Raygun.Blazor.Maui` from NuGet.

### Setup

Add a scoped `RaygunBlazorClient` by calling to `UseRaygunBlazorMaui()` with your `MauiApp` builder.

```cs
var builder = MauiApp.CreateBuilder();

builder
  .UseMauiApp<App>()
  // ... Other configuration
  .UseRaygunBlazorMaui();
```

### Capturing unhandled exceptions

Use `RaygunErrorBoundary` to wrap compoments and capture unhandled exceptions automatically.

For example, in your `Components/Layout/MainLayout.razor`:

```cs
@using Raygun.Blazor.Maui

...

<article class="content px-4">
  <RaygunErrorBoundary>
    @Body
  </RaygunErrorBoundary>
</article>
```

### Example

Example project is located in `src/Raygun.Samples.Blazor.Maui`

To run the example:

1. Install `dotnet-sdk` minimum version supported in `8.0.300`.
2. Setup the required .Net MAUI platform frameworks.
3. Add the `ApiKey` property to in `src/Raygun.Samples.Blazor.Maui/appsettings.json`

```
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

4. Run `dotnet build -t:Run -f framework` from the example folder, replacing `framework` by the desired framework (e.g. `net8.0-windows`).

The sample application will launch on the corresponding framework platform or device.

---

## Development

### To build a local nuget package

- Open Visual Studio 22+
- Open the `Raygun.Blazor.sln` solution
- Right-click the project and select properties
- Ensure the produce a NuGet package build option is checked
- Under package, update the version name

Each time you build your project a .nupkg file will be created in your bin directory.
