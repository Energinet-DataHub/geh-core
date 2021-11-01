# Documentation

Notes regarding usage of the NuGet package `Energinet.DataHub.Core.FunctionApp.TestCommon` that is part of the `TestCommon` bundle.

The package contains reuseable code to help implementing xUnit integration tests of Energinet DataHub Azure Functions.

> We aim for documenting types using XML documentation comments, so be sure to also look at those.

## Prerequisites

This library contains [managers](#managers) that depends on certain tools beeing installed:

* [Azurite](https://github.com/Azure/Azurite)
* [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)
* [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver15)

The tools must be available on the developer machine as well as on any build agent that must be able to run the integration tests that depends on this library.

In order to run Azurite and Azure Functions Core Tools we also need Node.js. It is recommended to use `nvm` to handle this. See [nvm-windows](https://github.com/coreybutler/nvm-windows/wiki#installation).

## Concept

The following only introduce the types supporting integration testing an Azure Function at a high level.

> For a concrete implementation example take a look at the [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

### Managers

First of all we have a group of components that we use to *manage* ressources or tools. Each component can manage a certain kind of ressource/tool, and are named as `<ressource/tool-type>Manager`.

A manager is typically responsible for creating/destroying or starting/stopping the type it manages. As such it helps us control the life-cycle of that type.

It is preferable if a manager obey the following principles:

* DO NOT create/start the managed type in the manager constructor
* DO expose an async initialization method for creating/starting the managed type
* DO expose an async dispose method for destroying/stopping the managed type

By following these principles, it becomes easier to orchestrate the full flow of the integration tests and all dependencies.

Currently we have the following managers:

* `AzuriteManager`; this is used to start/stop Azurite (a cross platform storage emulator).
* `SqlServerDatabaseManager`; this is used to create/destroy local SQL databases. For each database type we have, we should implement a class that inherits from this manager.
* `FunctionAppHostManager`; this is used to start/stop an Azure Function using Azure Functions Core Tools. It can be the Azure Function we want to integration test, or just one that we depend on in our integration tests.

### Resource providers

The `ServiceBusResourceProvider` is more complex than a *manager*, so we named it differently.

It makes it easy to build a bunch of resources within the same Azure Service Bus namespace, and will automatically track and cleanup any resources created, when it is disposed.

Queues and topics created using the resource provider, will be created using a combination of a given prefix and a random suffix. This is to ensure multiple runs of the same tests can run in parallel without interferring.

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
