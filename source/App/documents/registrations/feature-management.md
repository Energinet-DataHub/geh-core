# Feature Management

Guidelines for Azure Function App's and ASP.NET Core Web API's on configuring and using feature management in combination with Azure App Configuration.

## Overview

- [Introduction](#introduction)
    - [Feature flags](#feature-flags)
    - [Disabled flags](#disabled-flags)
- [Guidelines](#guidelines)
    - [General principles](#general-principles)
    - [Document feature flags](#document-feature-flags)
    - [Recommended implementation pattern for feature flag management](#recommended-implementation-pattern-for-feature-flag-management)
- [Samples](#samples)
    - [Feature flag](#feature-flag)
    - [Disabled flag](#disabled-flag)
- ["How to" in tests](#how-to-in-tests)
    - [Changing application settings](#changing-application-settings)
    - [Managing Azure App Configuration](#managing-azure-app-configuration)
    - [Managing feature flags through IFeatureManager](#managing-feature-flags-through-ifeaturemanager)

## Introduction

In the code and documentation we demo two ways of branching code:

- *Feature flags*
- *Disabled flags*

### Feature flags

DataHub subsystems should use Microsoft Feature Management in combination with Azure App Configuration to support toggling feature flags at runtime.

> See also [Azure App Configuration - Feature Management](
<https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management>)

After following the configuration described in our [quick-start](../documentation.md#quick-start-for-application-startup), we can configure feature flags in two places:

- In App Settings (locally or in Azure App Service). The application must be restarted to update the feature flag value.
- In Azure App Configuration under Feature manager. The feature flag value is automatically refreshed and updated at runtime.

### Disabled flags

 A built-in functionality of Azure Function that can be used to disable code at the function level.

> See [How to disable functions in Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/disable-function).

Disabled flag are configured in App Settings (locally or in Azure App Service). The application must be restarted to update the disabled flag value.

## Guidelines

A few simple guidelines regarding the usage of feature flags.

### General principles

- :heavy_check_mark: DO keep the number of active feature flags low in an area at all times.
    - Aim for having short lived feature flags, and remove them as soon as they are obsolete.
- :heavy_check_mark: DO use feature flags to enable/disable functionality at a high level, like:
    - Enable/disable a functionality at an application level by using a *feature flag*.
    - Enable/disable an Azure Function using a *disabled flag*.
- :x: DO NOT use feature flags to enable/disable functionality at a low level, like:
    - Enable/disable functionality deep within a component.

### Document feature flags

- :heavy_check_mark: DO document all active feature flags within an area, in Confluence or other *easy to spot* place.
- :heavy_check_mark: DO document when a feature flag can be removed so we continuously have focus on keeping the number of active feature flags low.

### Recommended implementation pattern for feature flag management

The [samples](#samples) mentioned later follow this pattern so we will add links to the code for easy reference.

1) Create a root folder named `FeatureManagement` in the application.

   See [FeatureManagement](https://github.com/Energinet-DataHub/geh-core/tree/main/source/App/source/ExampleHost.FunctionApp01/FeatureManagement)

1) Create a new class file as `FeatureManagement\FeatureFlagNames.cs` to keep track of active feature flags.

   See [FeatureFlagNames.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp01/FeatureManagement/FeatureFlagNames.cs)

1) Create a new class file as `FeatureManagement\FeatureManagerExtensions.cs` and implement a method per active feature flag.

   See [FeatureManagerExtensions.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp01/FeatureManagement/FeatureManagerExtensions.cs)

1) To use feature flags in application code inject the `IFeatureManager` interface.

   See [FeatureManagementFunction.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp01/Functions/FeatureManagementFunction.cs)

1) In integration tests configure feature flags and Azure App Configuration as mentioned under [Managing Azure App Configuration](#managing-azure-app-configuration).

   See [FeatureManagementTests.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp.Tests/Integration/FeatureManagementTests.cs) and [ExampleHostsFixture.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp.Tests/Fixtures/ExampleHostsFixture.cs)

1) In unit tests control feature flags as mentioned under [Managing feature flags through IFeatureManager](#managing-feature-flags-through-ifeaturemanager).

   See [FeatureManagementTests.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp.Tests/Unit/FeatureManagementTests.cs) and [FeatureManagerStub.cs](https://github.com/Energinet-DataHub/geh-core/blob/main/source/App/source/ExampleHost.FunctionApp.Tests/Fixtures/FeatureManagerStub.cs)

## Samples

The following samples are implemented in the `ExampleHost.FunctionApp01`.

### Feature flag

This sample shows how we can use a feature flag in C#, and branch the code based on the feature flag value (enabled or disabled).

Even though we demonstrate how it is possible to use the Microsoft Feature Management libraries in an Azure Function App, the same code and principles work with other C# applications, like an ASP.NET Core Web API.

In this sample we imagine we have functionality at the application layer represented by the feature `UseGetMessage`. In the sample this functionality is implemented within the function `FeatureManagementFunction.GetMessage` and guarded by the feature flag `FeatureFlagNames.UseGetMessage`.

We can control whether the feature is enabled or not, by changing the value `FeatureManagement__UseGetMessage` in the `local.settings.json` file.

> The test configures the feature flag as an App Setting, but it could as well be configured in Azure App Configuration, in which case it wouldn't require a restart.

In the test class `FeatureManagementTests.GetMessage` we test scenarios where the feature flag is disabled, and later enabled.

See also [Changing application settings](#changing-application-settings)

### Disabled flag

This sample shows how we can disable a function using an app setting.

We can control whether the function `FeatureManagementFunction.CreateMessage` is active or not, by changing the value `AzureWebJobs.CreateMessage.Disabled` in the `local.settings.json` file.

In the test class `FeatureManagementTests.CreateMessage` we show how we can test the expected behaviour from an integration test.

See also [Changing application settings](#changing-application-settings)

## "How to" in tests

### Changing application settings

When we use an app setting to control the feature flag, we must configure the value before we start the function app host. In some situations this means we have to restart the host from tests. This can be costly if we have many tests and multiple scenarios, so we should always consider this, and aim for the least numbers of restarts for a complete test run.

The method `FunctionAppHostManager.RestartHostIfChanges` can help reduce the number of restarts, as it will only restart the host if the values given are actual different from the current loaded settings. This will only work if environment variables was set using the `FunctionAppHostSettings.ProcessEnvironmentVariables`.

### Managing Azure App Configuration

The Integration Test environment contains an Azure App Configuration that we can use from tests:

- The endpoint value for this resource is available in `IntegrationTestConfiguration.AppConfigurationEndpoint`, implemented in the `TestCommon` bundle.

- The `TestCommon` bundle also contains an `AppConfigurationManager` which can be used from tests to manage feature flags in this resource. With this we can create, delete and read feature flags.

- The manager has a `AppConfigurationManager.DisableProviderSettingName` constant containing the name of a setting that can be used to disable the Azure App Configuration provider.

The Azure App Configuration resource should however not be used from *application tests*, but instead be used to verify *component* code (like the `App` bundle).

Instead we recommend application tests disables the provider. These tests are often run in parallel in CI's, and since they would use the same Azure App Configuration, tests can interfere with each other, and cause builds to fail.

The recommended test configuration for a function app looks like the following (similar can be used for an ASP.NET Core Web API):

```cs
    // Feature Management => Azure App Configuration settings
    appHostSettings.ProcessEnvironmentVariables.Add(
        $"{AzureAppConfigurationOptions.SectionName}__{nameof(AzureAppConfigurationOptions.Endpoint)}",
        IntegrationTestConfiguration.AppConfigurationEndpoint);
    appHostSettings.ProcessEnvironmentVariables.Add(
        AppConfigurationManager.DisableProviderSettingName,
        "true");
```

### Managing feature flags through IFeatureManager

If we need to be control of feature flag values in unit tests, we can:

- Use the `Moq` framework and create a mock for `IFeatureManager` where we setup the method `IsEnabledAsync` to return the feature flag value we want, based on the parameter `feature`.

- Create a stub that implements `IFeatureManager` and returns the feature flag value we want, based on the parameter `feature`.

These techniques will work even when we use extensions of `IFeatureManager`.
