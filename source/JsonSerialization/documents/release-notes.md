# JsonSerialization Release Notes

## Version 4.0.0

- Upgrade from .NET 8 to .NET 9

## Version 3.0.3

- Update tj-actions to v46.0.1
- No functional change.

## Version 3.0.2

- Update .github referencess to v14
- No functional change.

## Version 3.0.1

- Bumped various NuGet packages to the latest versions.
- No functional changes.

## Version 3.0.0

- Bumped to .NET 8

## Version 2.2.13

- Bumped various NuGet packages to the latest versions.
- No functional changes.

## Version 2.2.12

- Bump System.Text.Json
- No functional change.

## Version 2.2.11

- No functional change.

## Version 2.2.10

- No functional change.

## Version 2.2.9

- No functional change.

## Version 2.2.8

- No functional change.

## Version 2.2.7

- No functional change.

## Version 2.2.6

- No functional change.

## Version 2.2.5

- No functional change.

## Version 2.2.4

- Add testresults to CI report

## Version 2.2.3

- No functional change.

## Version 2.2.2

- Bump version as part of pipeline change.

## Version 2.2.1

- Bump version as part of pipeline change

## Version 2.2.0

Add method to `IJsonSerializer`:

```csharp
        /// <summary>
        /// Parse the value representing a single JSON value into an object of the type <typeparam name="T" />.
        /// </summary>
        public Task<T> DeserializeAsync<T>(byte[] data);
```

## Version 2.1.1

- Bumped patch version as pipeline file was updated.

## Version 2.1.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

## Version 2.0.0

- Upgrade project to build .NET 6 instead of .NET Standard 2.1

## Version 1.0.3

- Bumped patch version as pipeline file was updated.

## Version 1.0.2

- Add serializer that can serialize to a stream SerializeAsync

## Version 1.0.1

- Bumped patch version as pipeline file was updated.

## Version 1.0.0

- Added `JsonSerializer`

## Version 0.0.1

- Preparing packages for initial release
