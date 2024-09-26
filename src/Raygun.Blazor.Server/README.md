# Raygun for Blazor Server

[Raygun](http://raygun.com) provider for Blazor Server.

Full usage instructions can be found in the [Raygun.Blazor](https://www.nuget.org/packages/Raygun.Blazor) package page.

## Installation

Install the packages `Raygun.Blazor` and `Raygun.Blazor.Server` from NuGet.

## Setup

Add a scoped `RaygunBlazorClient` by calling to `UseRaygunBlazor()` with your `WebApplication` builder.

```cs
var builder = WebApplication.CreateBuilder(args);

...

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

## Example

Example project is located in `src/Raygun.Samples.Blazor.Server`
