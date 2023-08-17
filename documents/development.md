# Development notes

Notes regarding the development of [Energinet-DataHub](https://github.com/Energinet-DataHub) NuGet packages, also called libraries. The packages contain reusable types, supporting the development of Energinet DataHub.

## NuGet package bundle

In the following we will use the term _NuGet package bundle_ when referring to a group of related NuGet packages, between which we want to use project references during development of the packages, and NuGet package references when they are published.

## Multiple NuGet packages in mono repository

It is important to read and understand this section before changing anything. The fundamental idea behind the organization of this repository, and how everything works together, is to support:

* [KISS principle](https://en.wikipedia.org/wiki/KISS_principle)
* Project references between related packages (NuGet package bundle)
* Easy and faster update between related packages (NuGet package bundle)
* Only releasing NuGet package bundles where the content has been changed

### Organization of packages

Packages must be created in subfolders to the `source` folder, where each subfolder contains a NuGet package bundle.

All project files belonging to a NuGet package bundle must be added to a `<bundle>.sln` solution, and organized similar to existing projects and files, with regards to solution folders.

Each bundle has its own:

* `documentation.md` file for documentation the bundle content and possible usage.
* (optional) `development.md` file for additional information (e.g. setup local environment) necessary to be able to develop the specific package or bundle.
* `release-notes.md` file for documentation of changes between each version.
* Source code and tests.

The following are shared:

* `development.md` (this) file for documenting how to develop the packages and bundles in general.
* Directory.Build.props
* stylecop.json

Here is an example of the package folder structure using the existing `TestCommon` and a `MyNewBundle` bundle:

``` txt
<root>
│
├───documents
│      development.md
│
└───source
    │  Directory.Build.props
    │  stylecop.json
    │
    ├───TestCommon
    │   │  TestCommon.sln
    │   │
    │   ├───documents
    │   │   │  development.md
    │   │   │  documentation.md
    │   │   │
    │   │   └───release-notes
    │   │          release-notes.md
    │   │
    │   └───source
    │       └───FunctionApp.TestCommon
    │       └───FunctionApp.TestCommon.Tests
    │       └───TestCommon
    │       └───TestCommon.Tests
    │
    └───MyNewBundle
        │  MyNewBundle.sln
        │
        ├───documents
        │   │  documentation.md
        │   │
        │   └───release-notes
        │          release-notes.md
        │
        └───source
            └───MyNewPackage
            └───MyNewPackage.Tests
```

### Dependencies, build and publish

Projects within a bundle must use _Project References_ when depending on each other. When doing so, developers must be careful, and seriously consider the best modularization of code into projects, and the impact it has on dependencies.

However, preferable only packages that has changes are actually published, and hence requires an updated version. If only the tests, or `*.md` files for a package has changes, then the package should not have to be published.

### Common project configuration

All packages must start with `Energinet.DataHub.Core` in their output name, namespace and package id.

Consider using shorter project file names and folders (see TestCommon example), and instead ensure to set the full names using project properties:

``` xml
  <PropertyGroup>
    <AssemblyName>Energinet.DataHub.Core.MyPackageName</AssemblyName>
    <RootNamespace>Energinet.DataHub.Core.MyPackageName</RootNamespace>
    ...
    <PackageId>Energinet.DataHub.Core.MyPackageName</PackageId>
  </PropertyGroup>
```

### XML documentation

In all packages we should aim for documenting types using [XML documentation comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/).

## Preparations for a new release

Only packages that has changes should be published as new versions.

To prepare any given package for a new release, do the following:

1. Open the `*.csproj` file and update the property `PackageVersion`.
   > Don't edit the property from the editor, it will not work correctly.
1. Update the `release-notes.md` file following the existing style and structure.
1. If we need to describe any migration steps required to move from one version to the next, then do one of the following:
   * If the description in short: put it in the `release-notes.md`.
   * If the description requires more than a few lines: put it in a separate `.md` file and link to it from the `release-notes.md`. Name the new `.md` file following the format `version_<major>_<minor>_<patch>.md`, and place it in the `release-notes` folder.

### Version standard

The NuGet packages must use [Semantic Versioning](https://semver.org/)

The version format is:
`<major>.<minor>.<patch>$(VersionSuffix)`

Packaging a _prerelease_ version can be done by ensuring the `$(VersionSuffix)` build variable is set to a value.

The following example will use `-alpha-01` as version suffix:

``` powershell
dotnet pack --configuration Release /p:versionsuffix=-alpha-01
```

## Testing a package locally

The following is a walkthrough of how we can test a NuGet package locally, before we publish it to others.

The following expects us to use a PowerShell console or similar, where the current directory is the location of the `<bundle>.sln` file:

1. Create a folder to use as the local NuGet source.

   Here we will use `C:\Projects\LocalNuGet`.

1. Build the NuGet packages.

   The following creates each NuGet package as prerelease versions in the specified `--output` location.

   ```powershell
   dotnet pack --output C:\Projects\LocalNuGet --configuration Release /p:versionsuffix=-alpha-01
   ```

1. (Optional) Investigate package content.

   We can rename a `*.nupkg` file to `*.zip` and open it to investigate its
content.

Using e.g. Visual Studio, we can now install the local NuGet packages to other projects for verification:

1. Add a new NuGet package source to Visual Studio:

   > **Important:** Do not check-in changes this might cause to any `nuget.config` files.

   * Select menu item Tools | NuGet Package Manager | Package Manager Settings.
   * Choose Package Sources.
   * Press the "+" (plus) button to add a new source and specify the following:
     * Name: `LocalNuGet`
     * Source: `C:\Projects\LocalNuget`
   * Close the dialog by pressing "OK" button.

1. Use local NuGet package.

   Add NuGet packages as usual, but be sure to select the `LocalNuGet` as `Package source` to see the local packages. If the package is a prerelease we must also ensure to check `Include prerelease`, otherwise it will not show up on the list.

For another walkthrough, see [How to test NuGet packages locally](https://dev.to/iamrule/a-guide-on-how-to-test-new-versions-of-your-nuget-packages-locally-1phk).

## Debugging a released package

Some IDE's support downloading symbol (`*.pdb`) files for NuGet packages, to allow developers a rich debugging experience where they can debug into release builds of packages.

To support this feature our builds must pack and push `.snupkg` files to NuGet.org symbol server.
> For details, see [Improved package debugging experience with the NuGet.org symbol server](https://devblogs.microsoft.com/nuget/improved-package-debugging-experience-with-the-nuget-org-symbol-server/).

### Debug with symbols in Visual Studio

To download and use symbols for debugging in Visual Studio, you must enable NuGet.org symbol server, and enable certain debugger features. See [Debug with symbols in Visual Studio](https://docs.microsoft.com/en-us/azure/devops/artifacts/symbols/debug-with-symbols-visual-studio?view=azure-devops).

> **Notice**
Unchecking "Enable Just My Code" will cause the download and load of lots of symbols when you run your debugging session, so it can impact your performance. But you can have it checked by default, and only uncheck it in those situations where you need to dig into the libraries.

If you have "Enable Just My Code" checked then, when you step into code, you will get a dialog asking if you want to download the source. It is possible to set breakpoints in the downloaded source as well.

## CodeCov

When a new libray is added to the repository, tracking of code coverage must be enabled. To do so, follow the steps below:

* Add a flag to `codecov.yml` with name and path corresponding to the library.

``` yml
    - name: <flag-name>
      paths:
      - source/<new-folder-name>
```

* Add the folder of any `*.Tests` project to the ignore list in `codecov.yml`.

``` yml
  - "source/<new-folder-name>/source/<test-folder-name>.Tests"
```

* In the pipeline step using the action named `Energinet-DataHub/.github/.github/actions/dotnet-solution-build-and-test` add a argument named `code_coverage_flags`, and assign it the value given to the flag in `codecov.yml`.


## Project file

When a new package is added to the repository, it is important that the project file has the necessary properties for the new package.

Example of what project properties that should be set:

``` xml
  <PropertyGroup>
    <PackageId>Energinet.DataHub.Core.(library_name).(package_name)</PackageId>
    <PackageVersion>1.0.0$(VersionSuffix)</PackageVersion>
    <Title>insert_title</Title>
    <Company>Energinet-DataHub</Company>
    <Authors>Energinet-DataHub</Authors>
    <PackageProjectUrl>https://github.com/Energinet-DataHub</PackageProjectUrl>
    <RepositoryUrl>https://github.com/Energinet-DataHub/geh-core</RepositoryUrl>
    <!-- PackageReleaseNotes:
      Is shown in Azure DevOps artifacts Release Notes section.
    -->
    <PackageReleaseNotes>
      [Release Notes](https://github.com/Energinet-DataHub/geh-core/blob/master/source/(library_name)/documents/release-notes/release-notes.md)
      [Documentation](https://github.com/Energinet-DataHub/geh-core/blob/master/source/(library_name)/documents/documentation.md)
    </PackageReleaseNotes>
    <!-- PackageDescription:
      Is shown in GitHub packages "About this package" section,
      and in Visual Studio package manager view.
    -->
    <PackageDescription>
      [Release Notes](https://github.com/Energinet-DataHub/geh-core/blob/master/source/(library_name)/documents/release-notes/release-notes.md)
      [Documentation](https://github.com/Energinet-DataHub/geh-core/blob/master/source/(library_name)/documents/documentation.md)
    </PackageDescription>
    <Description>package_description</Description>
    <PackageTags>energinet;datahub</PackageTags>
    <PackageLicenseExpression>Apache-2.0</PackageLicenseExpression>
    <PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>
  </PropertyGroup>

  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
    <!-- Disable warning on your public types/methods for not having added full documentation tags -->
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>
```

### SourceLink

We use `SourceLink` to allow debugging.

Install nuget package: `Microsoft.SourceLink.GitHub`

Add the following properties in the project file:

``` xml
  <!-- Enable Source Link (https://github.com/dotnet/sourcelink/blob/master/README.md) -->
  <PropertyGroup>
    <!-- Publish the repository URL in the built .nupkg (in the NuSpec <Repository> element) -->
    <PublishRepositoryUrl>true</PublishRepositoryUrl>
  </PropertyGroup>
```
