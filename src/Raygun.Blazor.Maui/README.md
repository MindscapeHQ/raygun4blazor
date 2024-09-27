# Raygun for MAUI Blazor Hybrid applications

[Raygun](http://raygun.com) provider for MAUI Blazor Hybrid applications.

Full usage instructions can be found in the [Raygun.Blazor](https://www.nuget.org/packages/Raygun.Blazor) package page.

As .Net MAUI Blazor Hybrid applications are composed of both MAUI and Blazor, you will have to set up Raygun for MAUI and Raygun for Blazor separately.

Check the package [Raygun4Maui](https://www.nuget.org/packages/Raygun4Maui/) for the MAUI setup instructions.

## Installation

Install the packages `Raygun.Blazor` and `Raygun.Blazor.Maui` from NuGet.

## Setup

Add a scoped `RaygunBlazorClient` by calling to `UseRaygunBlazorMaui()` with your `MauiApp` builder.

```cs
var builder = MauiApp.CreateBuilder();

builder
  .UseMauiApp<App>()
  // ... Other configuration
  .UseRaygunBlazorMaui();
```

## Capturing unhandled exceptions

Use `RaygunErrorBoundary` to wrap components and capture unhandled exceptions automatically.

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

## Example

An example project is located in `src/Raygun.Samples.Blazor.Maui`
