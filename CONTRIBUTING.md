# Contributing to Raygun4Flutter

## Project and library organisation

Building the project requires [.Net](https://dotnet.microsoft.com/en-us/download).

- `Raygun.Blazor`: Main Raygun Blazor package
- `Raygun.Blazor.Maui`: Specific code for Hybrid Blazor MAUI apps
- `Raygun.Blazor.Server`: Specific code for Blazor "Server" (Web) apps
- `Raygun.Blazor.WebAssembly`: Specific code for Blazor WebAssembly apps
- `Raygun.Samples.*`: All project samples
- `Raygun.Tests.*`: All project tests

## Building and running

The recommended IDE for working on this project is Visual Studio on Windows.

Some tasks can also be performed via command line on Linux or MacOS.

### Tests

To run tests, run `dotnet test`.

### Formatting

To format the code, run `dotnet format`.

### Running from Visual Studio

Run the examples directly from Visual Studio by opening the `Raygun.Blazor.sln`.

### Running from command-line

Run the example from command line by running `dotnet run` inside any example folder.

Specific instructions for each project can be found in the respective `README.md` files.

### To build a local nuget package

- Open Visual Studio
- Open the `Raygun.Blazor.sln` solution
- Right-click the project and select properties
- Ensure the produce a NuGet package build option is checked
- Under package, update the version name

Each time you build your project a `.nupkg` file will be created in your bin directory.

## How to contribute?

This section is intended for external contributors not part of the Raygun team.

Before you undertake any work, please create a ticket with your proposal,
so that it can be coordinated with what we're doing.

If you're interested in contributing on a regular basis,
please get in touch with the Raygun team.

### Fork the repository

Please fork the main repository from https://github.com/MindscapeHQ/raygun4blazor
into your own GitHub account.

### Create a new branch

Create a local branch off `main` in your fork,
named so that it explains the work in the branch.

Do not submit a PR directly from your `main` branch.

### Open a pull request

Submit a pull request against the main repositories' `main` branch. 

Fill the PR template and give it a title that follows the [Conventional Commits guidelines](https://www.conventionalcommits.org/en/v1.0.0/).

### Wait for a review

Wait for a review by the Raygun team.
The team will leave you feedback and might ask you to do changes in your code.

Once the PR is approved, the team will merge it.

