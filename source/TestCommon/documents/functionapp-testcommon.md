# Documentation

Notes regarding usage of the NuGet package `Energinet.DataHub.Core.FunctionApp.TestCommon` that is part of the `TestCommon` bundle.

The package contains reuseable code to help implementing xUnit integration tests of Energinet DataHub Azure Functions.

To see examples of how to use the components, look at their tests. We aim for
documenting types using XML documentation comments, so be sure to also look at those.

## Prerequisites

This library contains [managers](#managers) that depends on certain tools beeing installed:

* [Azurite](https://github.com/Azure/Azurite)
* [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15)

The tools must be available on the developer machine as well as on any build agent that must be able to run the integration tests that depends on this library.

### Developer machine

**Important:** If any version of Node is already installed, it must be uninstalled before proceeding.

The following commands can be executed in a Command Prompt, PowerShell or similar.

#### Install NVM for Windows

In order to run Azurite and Azure Functions Core Tools we also need Node.js. It is recommended to use `nvm` to handle this.

1. Install `nvm` as [documented here](https://github.com/coreybutler/nvm-windows#installation--upgrades)

Afterwards we can check our version using:

```Prompt
nvm version
```

#### Install Node and NPM using NVM

1. Open Command Prompt as Administrator and run the following:

    > Use of the `lts` (long-time-support version) parameter requires at least version 1.1.8 of `nvm`.

    ```Prompt
    nvm install lts
    nvm use <version>
    ```

Afterwards we can check our versions using:

```Prompt
node -v
npm -v
```

#### Install Azurite using NPM

1. Find possible versions:

    * [Latest version supported by NPM](https://www.npmjs.com/package/azurite?activeTab=versions)
    * To see if a given version is in prerelease or release use this [link](https://github.com/Azure/azurite/releases)

1. Open Command Prompt and run the following:

```Prompt
npm install -g azurite@<version>
```

#### Install Azure Functions Core Tools using NPM

1. Find possible versions:

    * [Latest version supported by NPM](https://www.npmjs.com/package/azure-functions-core-tools?activeTab=versions)
    * To see if a given version is in prerelease or release use this [link](https://github.com/Azure/azure-functions-core-tools/releases)

1. Open Command Prompt and run the following:

    ```Prompt
    npm install -g azure-functions-core-tools@<version>
    ```

#### Install SQL LocalDB 2019 (optional)

Some developer machines have SQL local DB installed already. We only need to do the following if our scripts or code depends on functionality only available in SQL 2019.

1. **!IMPORTANT!** Uninstall existing "Microsoft SQL Server Xxx LocalDB", where Xxx is the version.
 Older versions may have been installed if you have Visual Studio installed.

2. Download <https://go.microsoft.com/fwlink/?LinkID=866658>

3. Run and choose to just download "LocalDB" (SqlLocal.DB.msi) <https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15#install-localdb>

4. Run MSI installer.

5. In PowerShell or similar, use "SqlLocalDB" commands (<https://docs.microsoft.com/en-us/sql/relational-databases/express-localdb-instance-apis/command-line-management-tool-sqllocaldb-exe?view=sql-server-ver15>)

    ```PowerShell
    SqlLocalDB stop MSSQLLocalDB
    SqlLocalDB delete MSSQLLocalDB
    SqlLocalDB create MSSQLLocalDB
    ```

6. In VS - SQL Server Object Explorer disconnect from localdb and connect again.

#### Running integration tests in JetBrains Rider

In order to be able to run integration tests in Rider you must have the "Azure Toolkit for Rider" plugin installed. When logging in through `Tools > Azure > Azure Log In` you get the option of using Azure CLI. This way Rider looks for the accessTokens.json file containing authentication tokens to access the resources in you subscription.

As of version 2.30.0 of the Azure CLI the `az login` command no longer creates the accessTokens.json file needed for Rider to access your subscription. Hence [Azure CLI version 2.29.2](https://github.com/Azure/azure-cli/releases/download/azure-cli-2.29.2/azure-cli-2.29.2.msi) or earlier must be installed to get access to your subscriptions through Rider.

NOTE: If you get Status: 403 (Forbidden) when trying to run an integration test in Rider and have both Rider and Visual Studio installed the problem might be that you are not signed in to the correct azure account in visual studio (Tools -> Options -> Azure Service Authentication -> Account Selection)

## Concept

The following only introduce the types supporting integration testing an Azure Function at a high level.

> For a concrete implementation example take a look at the [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

### Integration Test environment

The *Integration Test environment* is a resource group containing shareable Azure resources to support various integration test scenarios. E.g. this resource group contains a Azure Service Bus namespace, so we don't have to spent time creating one in our tests.

Information necessary to connect to shared resources, are stored as secrets in a Key Vault within the same resource group.

The `IntegrationTestConfiguration` can be used to retrieve these secrets in integration test setup.

### Managers

First of all we have a group of components that we use to *manage* ressources or tools. Each component can manage a certain kind of ressource/tool, and are named as `<ressource/tool-type>Manager`.

A manager is typically responsible for creating/destroying or starting/stopping the type it manages. As such it helps us control the life-cycle of that type.

It is preferable if a manager obey the following principles:

* DO NOT create/start the managed type in the manager constructor
* DO expose an async initialization method for creating/starting the managed type
* DO expose an async dispose method for destroying/stopping the managed type

By following these principles, it becomes easier to orchestrate the full flow of the integration tests and all dependencies.

Currently we have the following managers:

* `AppConfigurationManager`; this is used to set/create/delete feature flags in Azure App Configuration.
* `AzuriteManager`; this is used to start/stop Azurite (a cross platform storage emulator). It can be used with or without OAuth. When using OAuth a test certificate will be installed the first time Azurite is started. See also [TestCertificate ReadMe.md](../source/FunctionApp.TestCommon/TestCertificate/ReadMe.md).
* `SqlServerDatabaseManager`; this is used to create/destroy local SQL databases. For each database type we have, we should implement a class that inherits from this manager.
* `FunctionAppHostManager`; this is used to start/stop an Azure Function using Azure Functions Core Tools. It can be the Azure Function we want to integration test, or just one that we depend on in our integration tests.
* `OpenIdJwtManager`; this is used to start/stop an OpenId configuration server using WireMock, and creating DH3 internal tokens that are valid according to the aforementioned OpenId configuration server. A test certificate will be installed the first time the server is started. See also [TestCertificate ReadMe.md](../source/FunctionApp.TestCommon/TestCertificate/ReadMe.md).

### Resource providers

A *resource provider* is more complex than a *manager*. Apart from beeing a builder pipeline it also knows tracks the resources that has been created, and ensures these resources are deleted or disposed as necessary.

The `ServiceBusResourceProvider` makes it easy to manage queues/topics/subscriptions within an existing Azure Service Bus namespace.

For details, see [servicebusresourceprovider.md](./servicebusresourceprovider.md).

The `EventHubResourceProvider` makes it easy to manage event hubs within an existing Azure Event Hub namespace.

For details, see [eventhubresourceprovider.md](./eventhubresourceprovider.md).

### Verify Service Bus messaging

Another component to help us perform integration tests involving Azure Service Bus, is the `ServiceBusListenerMock`. It allows us to setup expectations on messages send to topics/queues.

For details, see [servicebuslistermock.md](./servicebuslistenermock.md).

### Test classes and fixtures

Another important aspect is the use of xUnit *fixtures*.
> See [Shard context between tests](https://xunit.net/docs/shared-context)

We use fixtures to orchestrate the setup, execution and teardown of integration tests. While the fixture is this usage scenario is the orchestrator of the overall life-cycle, it should use *managers* to handle the life-cycle of individual dependencies.

It is preferable if a fixture used by integration tests obey the following principles:

* DO NOT perform any form of async work in the constructor (use InitializeAsync)
* DO implement the xUnit `IAsyncLifetime` interface
* DO perform setup logic in InitializeAsync
* DO perform teardown logic in DisposeAsync

#### `FunctionAppFixture`

If we only have to manage one Azure Function App, we can inherit from the `FunctionAppFixture`.

This class ensures we configure `FunctionAppHostManager` so that:

* The Azure Functions output log can always be seen in the xUnit test output and on the build agent.
* If we are executing tests in Debug mode, we can also see the Azure Functions output log in the Output window/console of the IDE.
* `FunctionAppHostSettings` can be configured in a `functionapphost.settings.json` file.

By inheriting from this we can override hooks and handle setup/teardown of additional dependencies, necessary four our specific function app. For this we typically use our managers in `OnInitializeFunctionAppDependenciesAsync` and `OnDisposeFunctionAppDependenciesAsync`.

#### `FunctionAppTestBase`

The `FunctionAppTestBase` is build to make it easy to implement integration tests of an Azure Function App.

We can inherit our test class from `FunctionAppTestBase` and specify the subclass type of the `FunctionAppFixture` implementation matching the specific Azure Function App. All tests within the test class will then have the dependencies prepared, as given by the `FunctionAppFixture` implementation, and can access the `Fixture` property for additional handling/manipulation.
