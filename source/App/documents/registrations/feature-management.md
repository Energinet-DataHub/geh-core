# Feature Management

Guidelines for Azure Function App's and ASP.NET Core Web API's on configuring and using feature management in combination with Azure App Configuration.

## Overview

- [Introduction](#introduction)
- [Guidelines](#guidelines)
    - [General principles](#general-principles)
    - [Document feature flags](#document-feature-flags)
- [Samples](#samples)
    - [Disabled flag](#disabled-flag)
    - [Feature flag](#feature-flag)
- ["How to" in tests](#how-to-in-tests)
    - [Changing application settings](#changing-application-settings)
    - [Managing Azure App Configuration](#managing-azure-app-configuration)

## Introduction

DataHub subsystems should use Microsoft Feature Management in combination with Azure App Configuration to support toggling feature flags at runtime.

> See also [Azure App Configuration - Feature Management](
<https://docs.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management>)

After following the configuration described in our [quick-start](../documentation.md#quick-start-for-application-startup), we can configure feature flags in two places:

- In App Settings (locally or in Azure App Service). The application must be restarted to update the feature flag value.
- In Azure App Configuration under Feature manager. The feature flag value is automatically refreshed and updated at runtime.

## Guidelines

A few simple guidelines worth considering.

### General principles

- DO keep the number of active feature flags low in an area at all times.
    - Aim for having short lived feature flags, and remove them as soon as they are obsolete.
- DO use feature flags to enable/disable functionality at a high level, like:
    - Enable/disable a function using a *disabled flag*.
    - Enable/disable a functionality at an application level by using a *feature flag*.
- DO NOT use feature flags to enable/disable functionality at a low level, like:
    - Enable/disable functionality deep within a component.

### Document feature flags

- DO document all active feature flags within an area, in Confluence or other *easy to spot* place.
- DO document when a feature flag can be removed so we continuously have focus on keeping the number of active feature flags low.

#### Example: *Active feature flags*

| Name | Purpose | Must be removed when |
| ---- | ------- | ------------------- |
| A name | A purpose | Explain the condition under which we can remove this feature flag again |

## Samples

The following samples are implemented in the `ExampleHost.FunctionApp01`.

### Disabled flag

This sample shows how we can disable a function using an app setting.

> See [How to disable functions in Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/disable-function)

We can control whether the function `FeatureManagementFunction.CreateMessage` is active or not, by changing the value `AzureWebJobs.CreateMessage.Disabled` in the `local.settings.json` file.

In the test class `FeatureManagementTests.CreateMessage` we show how we can test the expected behaviour from an integration test.

See also [Changing application settings](#changing-application-settings)

### Feature flag

This sample shows how we can use a feature flag in C#, and branch the code based on the feature flag value (enabled or disabled).

Even though we demonstrate how it is possible to use the Microsoft Feature Management libraries in an Azure Function App, the same code and principles work with other C# applications, like an ASP.NET Core Web API.

In this sample we imagine we have functionality at the application layer represented by the feature `UseGetMessage`. In the sample this functionality is implemented within the function `FeatureManagementFunction.GetMessage` and guarded by the feature flag `FeatureFlagNames.UseGetMessage`.

We can control whether the feature is enabled or not, by changing the value `FeatureManagement__UseGetMessage` in the `local.settings.json` file.

> The test configures the feature flag as an App Setting, but it could as well be configured in Azure App Configuration, in which case it wouldn't require a restart.

In the test class `FeatureManagementTests.GetMessage` we test scenarious where the feature flag is disabled, and later enabled.

See also [Changing application settings](#changing-application-settings)

## How to in tests

### Changing application settings

When we use an app setting to control the feature flag, we must configure the value before we start the function app host. In some situations this means we have to restart the host from tests. This can be costly if we have many tests and multiple scenarious, so we should always consider this, and aim for the least numbers of restarts for a complete test run.

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
