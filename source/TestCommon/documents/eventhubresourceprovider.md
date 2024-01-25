# EventHubResourceProvider

The `EventHubResourceProvider` and its related types, support us with the following functionality:

- The `EventHubResourceProvider` is the fluent API builder root. It automatically tracks and cleanup any resources created, when it is disposed.
- The `EventHubResourceBuilder` type encapsulate the creation of an event hub in an existing Azure Event Hub namespace.
- The `EventHubResource` type support lazy creation of a producer client.
- The `EventHubConsumerGroupBuilder` type to encapsulate the creation of `ConsumerGroup`'s and adding to event hubs during creation of these.

> For usage, see `EventHubResourceProviderTests` or [Aggregations](https://github.com/Energinet-DataHub/geh-aggregations) repository/domain.

## Resource names

Event hubs created using the resource provider will be created using a combination of a given prefix and a random suffix. This is to ensure multiple runs of the same tests can run in parallel without interfering.

## Operations

### `BuildEventHub()` and `CreateAsync()`

The fluent API chain always starts with a `BuildEventHub()` operation, and ends with `CreateAsync()`.

The `CreateAsync()` operation returns the resource type `EventHubResource`. This type give us access to the full resource name created, and a producer client configured to send events to the created resource.

### `AddConsumerGroup()`

When building an EventHub its possible to add consumer groups.

### `Do()` and `SetEnvironmentVariableTo` extensions

The `EventHubBuilder` type support the `Do()` operation which allows us to register *post actions*. Each post action will be called just after the event hub has been created, with the properties of the event hub.

On top of `Do()` we have implemented `SetEnvironmentVariableTo` extensions, which let us set a environment variable to the name of the just created event hub.

## Examples

Prepare resource provider:

```csharp
// Remember to call DisposeAsync() on the resource provider to delete resources and close producer client connections.
var integrationTestConfiguration = new IntegrationTestConfiguration();
var resourceProvider = new EventHubResourceProvider(
    integrationTestConfiguration.EventHubConnectionString,
    integrationTestConfiguration.ResourceManagementSettings,
    new TestDiagnosticsLogger());
```

Example 1 - creating an event hub:

```csharp
// Create an event hub prefixed with the name "eventhub" and a consumergroup with name "consumer_group" (without optional user metadata).
var eventHubResource = await resourceProvider
    .BuildEventHub("eventhub")
    .AddConsumerGroup("consumer_group")
    .CreateAsync();

// We can access the full event hub name...
var eventHubName = eventHubResource.Name;
// ... and get an event hub producer client.
var producerClient = eventHubResource.ProducerClient;
```

Example 2 - creating an event hub, and set its name as a value in an environment variable:

```csharp
// Create an event hub prefixed with the name "eventhub".
var eventHubResource = await resourceProvider
    .BuildEventHub("eventhub").SetEnvironmentVariableToEventHubName("ENV_EVENTHUB")
    .CreateAsync();
```

Clean up:

```csharp
// Delete resources and close any created producer clients.
await resourceProvider.DisposeAsync();
```
