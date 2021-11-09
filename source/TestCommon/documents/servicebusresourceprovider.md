# ServiceBusResourceProvider

The `ServiceBusResourceProvider` and its related types, support us with the following functionality:

- The `ServiceBusResourceProvider` is the fluent API builder root. It automatically tracks and cleanup any resources created, when `ServiceBusResourceProvider` is disposed.
- The `Builder` types encapsulates the creation of queues/topics/subscriptions in an existing Azure Service Bus namespace.
- The `Resource` types support lazy creation of matching sender clients.

> For usage, see `ServiceBusResourceProviderTests` or [Charges](https://github.com/Energinet-DataHub/geh-charges) repository/domain.

## Resource names

Queues and topics created using the resource provider, will be created using a combination of a given prefix and a random suffix. This is to ensure multiple runs of the same tests can run in parallel without interferring.

Subscriptions will be created with the name given as parameter and does not contain a random suffix.

## Automatically cleanup Azure Service Bus resources

Even if the created queues/topics are not deleted explicit by calling dispose on the resource provider, they will still be deleted after an idle timeout.

See [AutoDeleteOnIdleTimeout](../source/FunctionApp.TestCommon/ServiceBus/ResourceProvider/ServiceBusResourceProvider.cs).

## Examples

Prepare resource provider:

```csharp
// Remember to call DisposeAsync() on the resource provider to delete resources and close sender client connections.
var integrationTestConfiguration = new IntegrationTestConfiguration();
var resourceProvider = new ServiceBusResourceProvider(
    integrationTestConfiguration.ServiceBusConnectionString,
    new TestDiagnosticsLogger());
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

Clean up:

```csharp
// Delete resources and close any created sender clients.
await resourceProvider.DisposeAsync();
```
