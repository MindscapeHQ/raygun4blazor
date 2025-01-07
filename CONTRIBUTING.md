# Development

## To build a local nuget package

- Open Visual Studio 22+
- Open the `Raygun.Blazor.sln` solution
- Right-click the project and select properties
- Ensure the produce a NuGet package build option is checked
- Under package, update the version name

Each time you build your project a .nupkg file will be created in your bin directory.

## Release process

- Increase version in `src/Version.props`
- Update `CHANGELOG.md`
- Build package in release mode. e.g:

```
dotnet build --configuration Release Raygun.Blazor
```

- Pack NuGet package. e.g:

```
dotnet pack Raygun.Blazor
```

- Upload NuGet package to nuget.org. 
  e.g: Package located in `src/(project)/bin/Release`.
