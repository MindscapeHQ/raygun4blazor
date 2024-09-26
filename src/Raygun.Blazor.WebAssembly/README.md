# Raygun for Blazor WebAssembly

[Raygun](http://raygun.com) provider for Blazor WebAssembly.

Full usage instructions can be found in the [Raygun.Blazor](https://www.nuget.org/packages/Raygun.Blazor) package page.

## Installation

Install the packages `Raygun.Blazor` and `Raygun.Blazor.WebAssembly` from NuGet.

## Setup

Add a scoped `RaygunBlazorClient` by calling to `UseRaygunBlazor()` with your `WebAssemblyHostBuilder` builder.

```cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ...

builder.UseRaygunBlazor();
```

## Accessing `RaygunBlazorClient`

You can access the `RaygunBlazorClient` using `@inject` in your code:

```cs
@inject RaygunBlazorClient RaygunClient

...

RaygunClient.RecordExceptionAsync(...)
```

## Capturing unhandled exceptions

Use `RaygunErrorBoundary` to wrap components and capture unhandled exceptions automatically.

For example, in your `MainLayout.razor`:

```cs
@using Raygun.Blazor.WebAssembly.Controls

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

## Example

Example project is located in `src/Raygun.Samples.Blazor.WebAssembly`

