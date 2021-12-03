# Quickstarts: Feature flag

## Overview

- Preparing an Azure Function App project
- Adding a new feature flag
- Using a feature flag

For testing, see [Samples: Feature flag](./samples.md#feature-flag).

## Preparing an Azure Function App project

1) Install this NuGet package:
   `Microsoft.FeatureManagement`

1) Add the following to a *ConfigureServices()* method in Program.cs:
   `builder.Services.AddFeatureManagement();`

1) Create a new class file as `Common\FeatureFlags.cs` with the following content:

   ```cs
   /// <summary>
   /// Feature flags container, to avoid using magic strings for flags.
   /// </summary>
   public static class FeatureFlags
   {
       /// <summary>
       /// On Windows it is also possible to use ":" as separator, but "__" is supported on multiple platforms.
       /// </summary>
       public const string ConfigurationPrefix = "FeatureManagement__";

       /// <summary>
       /// Current feature flags.
       /// </summary>
       public enum Names
       {
       }
   }
   ```

## Adding a new feature flag

1) Add an entry to the `FeatureFlags.Names` enum with the name of the feature flag.

   ```cs
   public static class FeatureFlags
   {
    ...
       public enum Names
       {
           UseGuidMessage,
       }
   }
   ```

1) For local development, add an entry in the `Values` section of `local.settings.sample.json` and `local.settings.json`:

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       ...

       // On Windows it is also possible to use ":" as separator, but "__" is supported on multiple platforms.
       "FeatureManagement__UseGuidMessage": false
     }
   }
   ```

## Using a feature flag

To access the value of a feature flag in a class, extend the constructor to require an additional argument of type `IFeatureManager` and store a reference to this instance.

Then, where code paths need to be determined by a feature flag, invoke the `IFeatureManager`as given below:

   ``` csharp
   var isFeatureEnabled = await FeatureManager.IsEnabledAsync(nameof(FeatureFlags.Names.UseGuidMessage));
   if (isFeatureEnabled)
   {
       // Will be executed if feature flag is enabled
   }
   ```
