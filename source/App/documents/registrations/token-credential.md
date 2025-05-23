# Token Credential

## Overview

- Implementation
    - [Token credential factory](#token-credential-factory)
    - [Token credential provider](#token-credential-provider)

## Token credential factory

When accessing Azure resources (like service bus) or other subsystems, we use implementations of `TokenCredential`. The token credential implementation is then used by various classes to retrieve tokens for authentication and authorization.

The Azure App Services, in which we run our applications, are configured to use managed identity. So when we run in Azure we want to use the `ManagedIdentityCredential` implementation for retrieving tokens on behalf of the application.

When running integration/subsystem tests locally, or in CI workflows, we want to retrieve tokens as the identity which is executing the tests (developer or service principal). So when we are not running in Azure we want to use the `DefaultAzureCredential` implementation. When creating the `DefaultAzureCredential` we disable authentication mechanisms that we know we don't use/need, because it will try all enabled authentication mechanisms and we want to save time whenever possible.

To encapsulate this logic we have implemented the static `TokenCredentialFactory.CreateCredential()`.

## Token credential provider

In most places where we need `TokenCredential` we have access to `IServiceProvider`.

To make it simple and easy to get a correct configured token credential, we have implemented the extension method `IdentityExtensions.AddTokenCredentialProvider` which registers the `TokenCredentialProvider` as singleton. The method `AddTokenCredentialProvider()` is idempotent.

Any where we have access to `IServiceProvider` and need `TokenCredential`, we should use `TokenCredentialProvider` to get access to the registered `TokenCredential`.

> If we need `TokenCredential` in a places where we haven't got `IServiceProvider` then at least always ensure to use `TokenCredentialFactory.CreateCredential()` to get an instance. This ensure we still use the best choice, from a security point of view, when running in Azure.
