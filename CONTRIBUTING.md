# Development

## To build a local nuget package

- Open Visual Studio 22+
- Open the `Raygun.Blazor.sln` solution
- Right-click the project and select properties
- Ensure the produce a NuGet package build option is checked
- Under package, update the version name

Each time you build your project a .nupkg file will be created in your bin directory.

## Development on Linux

- Use JetBrains Rider.
- MAUI projects (`Raygun.Blazor.Maui` and `Raygun.Samples.Blazor.Maui`) won't compile on Linux, as it is not supported, even when the Android SDK is setup on Rider. See https://www.jetbrains.com/help/rider/MAUI.html#i22k0dp_42
- "Unload" those projects to avoid having compilation errors.
