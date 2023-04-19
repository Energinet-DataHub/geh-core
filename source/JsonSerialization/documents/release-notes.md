# JsonSerialization Release Notes

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
