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

First of all we have a group of types that we use to *manage* ressources. Each type can manage a certain kind of ressource, and are named as `<ressource-type>Manager`.

A manager is typically responsible for creating/destroying or starting/stopping the type it manages. As such it helps us control the life-cycle of that ressource.

It is preferable if a manager obey the following principles:
* DO NOT create/start ressources in the manager constructor
* DO expose an async initialization method for creating/starting ressources
* DO expose an async dispose method for destroying/stopping ressources

By following these principles, it becomes easier to orchestrate the full flow of the integration tests and all dependencies.

Currently we have the following managers:

* `AzuriteManager`; this is used to start/stop Azurite (a cross platform storage emulator).
* `SqlServerDatabaseManager`; this is used to create/destroy local SQL databases. For each database type we have, we should implement a class that inherits from this manager.
* `FunctionAppHostManager`; this is used to start/stop an Azure Function using Azure Functions Core Tools. It can be the Azure Function we want to integration test, or just one that we depend on in our integration tests. 

#### `FunctionAppHostManager`

Using the `FunctionAppHostManager` we get additional benefits:
* The Azure Functions output log can always be seen in the xUnit test output and on the build agent.
* If we are executing tests in Debug mode, we can also see the Azure Functions output log in the Output window/console of the IDE.

### Service Bus listener mock

The Service Bus listener mock allows us to use a fluent API to setup expectations on messages sent to a topic or queue.

See [servicebuslistermock.md](./servicebuslistenermock.md) for a brief explanation on usage.

### Test classes and fixtures

*TODO: Mention usage of the `functionapphost.settings.json`.*

*TODO: Mention function host can be debugged while performing integration tests during a Debug session.*
