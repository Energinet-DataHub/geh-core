# Documentation

Notes regarding usage of the NuGet package `Energinet.DataHub.Core.DurableFunctionApp.TestCommon` that is part of the `TestCommon` bundle.

The package contains reuseable code to help implementing xUnit integration tests of Energinet DataHub Durable Functions.

To see examples of how to use the components, look at their tests. We aim for
documenting types using XML documentation comments, so be sure to also look at those.

## Managers

First of all we have a group of components that we use to *manage* ressources or tools. Each component can manage a certain kind of ressource/tool, and are named as `<ressource/tool-type>Manager`.

A manager is typically responsible for creating/destroying or starting/stopping the type it manages. As such it helps us control the life-cycle of that type.

It is preferable if a manager obey the following principles:

* DO NOT create/start the managed type in the manager constructor
* DO expose an async initialization method for creating/starting the managed type
* DO expose an async dispose method for destroying/stopping the managed type

By following these principles, it becomes easier to orchestrate the full flow of the integration tests and all dependencies.

Currently we have the following managers:

* `DurableTaskManager`; this is used to create/destroy instances of IDurableClient.
