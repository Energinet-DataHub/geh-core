# Samples

## Overview

- [Disabled flag](#disabled-flag)
- [Feature flag](#feature-flag)
- [How to guides](#how-to-guides)
  - [Changing application settings](#changing-application-settings)

## Disabled flag

This sample shows how we can disable a function using an app setting.

> See [How to disable functions in Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/disable-function)

We can control whether the function `FeatureFlaggedFunction.CreateMessage` is active or not, by changing the value `AzureWebJobs.CreateMessage.Disabled` in the `local.settings.json` file.

In the test class `FeatureFlaggedFunctionTests.CreateMessage` we show how we can test the expected behaviour from an integration test.

See also [Changing application settings](#changing-application-settings)

## Feature flag

This sample shows how we can enable/disable a feature in C# using an app setting.

> The Microsoft Feature Management libraries are usually used with Azure App Configuration, but as we show in the sample they can be used for simple scenarios in isolation. See also [Azure App Configuration - Feature Management](
https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management)

Even though we demonstrate how it is possible to use the Microsoft Feature Management libraries in an Azure Function App, the same code and principles should work with other C# applications, like an ASP.NET Core Web API.

In this sample we imagine we have functionality at the application layer represented by the feature `UseGuidMessage`. In the sample this functionality is implemented within the function `FeatureFlaggedFunction.GetMessageAsync` and guarded by the feature flag `FeatureFlags.Names.UseGuidMessage`.

We can control whether the feature is enabled or not, by changing the value `FeatureManagement__UseGuidMessage` in the `local.settings.json` file.

In the test class `FeatureFlaggedFunctionTests.GetMessageAsync_UseGuidMessageFeatureFlagIsFalse` we test scenarious where the feature flag is disabled.

In the test class `FeatureFlaggedFunctionTests.GetMessageAsync_UseGuidMessageFeatureFlagIsTrue` we test scenarious where the feature flag is enabled.

See also [Changing application settings](#changing-application-settings)

## How to guides

### Changing application settings

Since we use an app setting to control the behaviour, we must configure the value before we start the function app host. In some situations this means we have to restart the host from tests. This can be costly if we have many tests and multiple scenarious, so we should always consider this, and aim for the least numbers of restarts for a complete test run.

The method `FunctionAppHostManager.RestartHostIfChanges` can help reduce the number of restarts, as it will only restart the host if the values given are actual different from the current loaded settings. This will only work if environment variables was set using the `FunctionAppHostSettings.ProcessEnvironmentVariables`.
