# Raygun for Blazor WebAssembly example

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
