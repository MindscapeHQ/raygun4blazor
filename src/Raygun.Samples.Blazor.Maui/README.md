# Raygun for MAUI Blazor Hybrid example

To run the example:

1. Install `dotnet-sdk` minimum version supported in `10.0.100`.
2. Set up the required .Net MAUI platform frameworks.
3. Add the `ApiKey` property to in `src/Raygun.Samples.Blazor.Maui/appsettings.json`

```
{
  "Raygun": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

4. Run `dotnet build -t:Run -f <framework>` from the example folder, replacing `<framework>` by the desired framework (e.g. `net10.0-windows`).

The sample application will launch on the corresponding framework platform or device.
