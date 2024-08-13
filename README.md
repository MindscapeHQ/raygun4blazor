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

- [ ] TODO: Add more info. See `src/Raygun.Blazor/RaygunBlazorClient.cs` for more information.

### Recording an error

Call to `raygunClient.RecordExceptionAsync(...)`

- [ ] TODO: Add more info. See `src/Raygun.Blazor/RaygunBlazorClient.cs` for more information.

### Recording a breadcrumb

Call to `raygunClient.RecordBreadcrumb(...);`

- [ ] TODO: Add more info. See `src/Raygun.Blazor/RaygunBlazorClient.cs` for more information.

### `RaygunErrorBoundary`

- [ ] TODO: Document when to use the `RaygunErrorBoundary`.

Currently used in `src/Raygun.Samples.Blazor.WebAssembly/App.razor`

### Attaching user details

Raygun for Blazor provides two ways to attach user details to error reports:

1. Provide `UserDetails` in the `RecordExceptionAsync` method call.
2. Implement a `IRaygunUserManager`.

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

#### Implementing `IRaygunUserManager`

Providing an instance of `IRaygunUserManager` to the Raygun Blazor client allows you to attach user details also to errors reported automatically, for example, captured unhandled exceptions or exceptions from the JavaScript layer.

Implement an `IRaygunUserManager`, for example:

```cs
public class MyUserManager : IRaygunUserManager
{
    public Task<UserDetails?> GetCurrentUser()
    {
        return Task.FromResult(new UserDetails());
    }
}
```

And inject it into the Raygun Blazor client:

```cs
builder.Services.AddSingleton<IRaygunUserManager, MyUserManager>();
```

For a complete example on how to implement a `IRaygunUserManager` with the `AuthenticationStateProvider` check the example project file `src/Raygun.Samples.Blazor.WebAssembly/Program.cs`.

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

- [ ] TODO: NuGet install instructions

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

---

## Publishing

- [ ] TODO: Packagre publishing instructions, e.g. NuGet publish instructions
