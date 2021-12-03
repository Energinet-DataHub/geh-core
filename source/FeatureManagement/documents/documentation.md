# Documentation

This is a sample showing how we by use of the Microsoft Feature Management libraries, and a few common guidelines, can support feature flags in an Azure Functions App.

It is meant to be a simple solution, as we aim for a minimum viable product. Even though the sample demonstrate feature flagging in an Azure Function App, the same code and principles should work with other C# applications, like an ASP.NET Core Web API.

> The Microsoft Feature Management libraries are usually used with Azure App Configuration, but as we show in the sample they can be used for simple scenarios in isolation. See also [Azure App Configuration - Feature Management](
https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)

## Guidelines

### Document feature flags

All active feature flags should be documented in `development.md` in an *Active feature flags* section close to the top. 

This is to ensure all developer are aware of any current feature flags, and to continuously have focus on keeping a low number of active feature flags (to reduce complexity).

| Name | Purpose | Must be removed when |
| ---- | ------- | ------------------- |
| A name | A purpose | Explain the condition under which we can remove this feature flag again |

## Sample

See the `SampleApp` and `SampleApp.Tests` code.

## Walkthrough

### Preparing an Azure Function App project

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

### Adding a new feature flag

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

### Using a feature flag

To access the value of a feature flag in a class, extend the constructor to require an additional argument of type `IFeatureManager` and store a reference to this instance.

Then, where code paths need to be determined by a feature flag, invoke the `IFeatureManager`as given below:

   ``` csharp
   var isFeatureEnabled = await FeatureManager.IsEnabledAsync(nameof(FeatureFlags.Names.UseGuidMessage));
   if (isFeatureEnabled)
   {
       // Will be executed if feature flag is enabled
   }
   ```

### Integration testing with feature flags

TODO: Add description

### Toggling a feature flag

TODO: Add description

### Removing a feature flag

TODO: Add description