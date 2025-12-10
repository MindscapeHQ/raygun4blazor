# Releasing Raygun4Blazor

Raygun for Blazor is published on NuGet as [`Raygun.Blazor`](https://www.nuget.org/packages/Raygun.Blazor).

## Semantic versioning

This package follows semantic versioning,

Given a version number MAJOR.MINOR.PATCH (x.y.z), increment the:

- MAJOR version when you make incompatible changes
- MINOR version when you add functionality in a backward compatible manner
- PATCH version when you make backward compatible bug fixes

To learn more about semantic versioning check: https://semver.org/

## Preparing for release

### Release branch

Create a new branch named `release/x.y.z` 
where `x.y.z` is the Major, Minor and Patch release numbers.

### Update version

Update the `Version` and `PackageVersion` in the `src/Version.props` file.

### Update CHANGELOG.md

Add a new entry in the `CHANGELOG.md` file.

Obtain a list of changes using the following git command:

```
git log --pretty=format:"- %s (%as)"
```

### Commit and open a PR

If everything succeeded, commit all the changes into a commit with the message `chore: Release x.y.z`
where `x.y.z` is the Major, Minor and Patch release numbers.

Then push the branch and open a new PR, ask the team to review it.

## Publishing

### PR approval

Once the PR has been approved, you can publish the provider.

### Package download

The "Build and Pack NuGet Packages" CI Action takes care of building the packages and archiving the files.

To download the archived packages:

1. Go to the [Actions](https://github.com/MindscapeHQ/raygun4blazor/actions) tab on GitHub.
2. Select the most recent "Build and Pack NuGet Packages" workflow run for your release branch.
3. Scroll down to the "Artifacts" section at the bottom of the workflow run summary.
4. Click on the artifact(s) to download the packaged `.nupkg` files.

### Upload to NuGet

You will need https://www.nuget.org/ credentials to publish, 
as well as being part of the [Raygun organization](https://www.nuget.org/profiles/Raygun).

Upload the packages generated previously.

Uploading the package is a manual process, and needs to be done for the four packages.

### Merge PR to main

With the PR approved and the packages published, 
squash and merge the PR into `main`.

### Tag and create Github Release

Go to https://github.com/MindscapeHQ/raygun4blazor/releases and create a new Release.

GitHub will create a tag for you, you don't need to create the tag manually.

You can also generate the release notes automatically.
