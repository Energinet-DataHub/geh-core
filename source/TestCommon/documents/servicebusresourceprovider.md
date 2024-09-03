# ServiceBusResourceProvider

The `ServiceBusResourceProvider` and its related types, support us with the following functionality:

- The `ServiceBusResourceProvider` is the fluent API builder root. It automatically tracks and cleanup any resources created, when it is disposed.
- The `Builder` types encapsulate the creation of queues/topics/subscriptions in an existing Azure Service Bus namespace.
- The `Resource` types support lazy creation of matching sender clients.

> For usage, see `ServiceBusResourceProviderTests` or [EDI](https://github.com/Energinet-DataHub/opengeh-edi) repository/subsystem.

## Resource names

Queues and topics created using the resource provider will be created using a combination of a given prefix and a random suffix. This is to ensure multiple runs of the same tests can run in parallel without interfering.

Subscriptions will be created with the name given as parameter and does not contain a random suffix.

## Automatically cleanup Azure Service Bus resources

Even if the created queues/topics are not deleted explicitly by calling dispose on the resource provider, they will still be deleted after an idle timeout.

See [AutoDeleteOnIdleTimeout](../source/FunctionApp.TestCommon/ServiceBus/ResourceProvider/ServiceBusResourceProvider.cs).

## Operations

### `BuildQueue()`, `BuildTopic()` and `CreateAsync()`

The fluent API chain always starts with a `Build()` operation, and ends with `CreateAsync()`.

The `CreateAsync()` operation returns one of the resource types `QueueResource` or `TopicResource`. These `Resource` types give us access to the full resource name created, and a sender client configured to send messages to the created resource.

The `TopicResource` also has a readonly collection of subscriptions added.

Depending on the resource type being built, additional operations are available.

### `AddSubscription()`

When building a topic its possible to also add subscriptions.

### `AddRule()`,  `AddSubjectFilter()`,  `AddSubjectAndToFilter()`

When building a subscriptions it is possible to add rules and filters.

### `Do()` and `SetEnvironmentVariableTo` extensions

All `Builder` types support the `Do()` operation which allows us to register *post actions*. Each post action will be called just after the resource type has been created, with the properties of the resource type.

On top of `Do()` we have implemented `SetEnvironmentVariableTo` extensions, which let us set a environment variable to the name of the just created resource.

## Examples

Prepare resource provider:

```csharp
// Remember to call DisposeAsync() on the resource provider to delete resources and close sender client connections.
var integrationTestConfiguration = new IntegrationTestConfiguration();
await using var resourceProvider = new ServiceBusResourceProvider(new TestDiagnosticsLogger(), 
    integrationTestConfiguration.ServiceBusFullyQualifiedNamespace);
```

Example 1 - creating a topic with two subscriptions:

```csharp
// Create a topic prefixed with the name "topic", and two subscriptions.
var topicResource = await resourceProvider
    .BuildTopic("topic")
    .AddSubscription("subscription01")
    .AddSubscription("subscription02")
    .CreateAsync();

// We can access the full topic name...
var topicName = topicResource.Name;
// ... and get a topic sender client.
var topicSenderClient = topicResource.SenderClient;
```

Example 2 - creating a topic with a single subscription, and set their names as values in environment variables:

```csharp
// Create a topic prefixed with the name "topic", and one subscription.
var topicResource = await resourceProvider
    .BuildTopic("topic").SetEnvironmentVariableToTopicName("ENV_TOPIC")
    .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName("ENV_TOPIC_SUBSCRIPTION")
    .CreateAsync();
```

Example 3 - creating a subscription with a subject filter

```csharp
var topicResource = await resourceProvider
    .BuildTopic("topic")
    .AddSubscription("subscription")
    .AddSubjectFilter("message-subject")
    .CreateAsync();
```

Example 4 - creating a subscription with a message type filter

```csharp
var topicResource = await resourceProvider
    .BuildTopic("topic")
    .AddSubscription("subscription")
    .AddMessageTypeFilter("some-message-type")
    .CreateAsync();
```

Clean up:

```csharp
// Delete resources and close any created sender clients.
await resourceProvider.DisposeAsync();
```
