# ServiceBusListenerMock

The `ServiceBusListenerMock` makes it easy to test and verify code that sends messages to a Azure Service Bus queue or topic.

The `ServiceBusListenerMock` connects to an actual Azure Service Bus instance. Using a fluent API developers can add listeners and register message handlers for received messages, similar to setting up verifications on a mock. So basically developers tells what message or messages they expect to receive, and what should happen when a message is received.

> For usage, see `ServiceBusListenerMockTests` or [EDI](https://github.com/Energinet-DataHub/opengeh-edi) repository/subsystem.

## Replay of messages

Already received messages will be "replayed" on any new message handler that is registered.

This allows us to register message handlers and get notified correctly, even after the listeners has started receiving messages.

## Operations

### `AddQueueListenerAsync()` and `AddTopicSubscriptionListenerAsync()`

Add a listener to start receiving messages from either a queue and/or a topic/subscription pair.

### `When()` and `DoAsync()`

At the lowest level we can use `When()` to setup a filter based on any service bus message property. If an incoming message matches the filter, the action registered with `DoAsync()` is executed.

```csharp
await serviceBusListenerMock
    .When(receivedMessage =>
        receivedMessage.MessageId == expectedMessageId
        && receivedMessage.Subject == expectedSubject)
    .DoAsync(_ =>
    {
        // Do something here...
        return Task.CompletedTask;
    });
```

#### Extensions

On top of `When()` and `DoAsync()` we have implemented several extensions to make the usage simpler.

When extensions:

- `WhenAny()`: When any message arrive.
- `WhenMessageId(expectedMessageId)`: When a message arrives with expected message id.
- `WhenSubject(expectedSubject)`: When a message arrives with expected subject.

Do extensions:

- `VerifyOnceAsync()`: Returns a reset event that can be waited, and that will be signaled when a message matching the *When* filter arrives.
- `VerifyOnceAsync(messageHandler)`: Overloaded version that will also call `messageHandler`.
- `VerifyCountAsync(expectedCount)`: Returns a countdown event that can be waited, and that will be signaled when the expected number of messages matching the *When* filter has arrived.
- `VerifyCountAsync(expectedCount, messageHandler)`: Overloaded version that will also call `messageHandler`.

### `ReceivedMessages`

Readonly collection that contains a clone of the messages that has arrived.

### `ResetMessageHandlersAndReceivedMessages()`

The service bus mock can be reset to clear all handlers and already received messages.
Use this between tests.

### `ResetMessageReceiversAsync`

The service bus listener mock can be reset to clear all listeners.
We should only use this if we are also removing the queue or topic.

## Examples

Prepare service bus listener mock:

```csharp
// Remember to Dispose() the mock to shutdown listeners and close connections.
var integrationTestConfiguration = new IntegrationTestConfiguration();
await using var serviceBusListenerMock = new ServiceBusListenerMock(new TestDiagnosticsLogger(), integrationTestConfiguration.ServiceBusFullyQualifiedNamespace);

// Add a listener.
// Here we use a queue listener, but a topic/subscription listener is also supported.
await serviceBusListenerMock.AddQueueListenerAsync(queueName);
```

Example using `WhenMessageId()` and `VerifyOnceAsync()` extensions:

```csharp
// E.g. reset between tests by adding this to test class constructor.
serviceBusListenerMock.ResetMessageHandlersAndReceivedMessages();

// Setup verification.
// Here we expect a message with message id to arrive.
using var isReceivedEvent = await serviceBusListenerMock
    .WhenMessageId(expectedMessageId)
    .VerifyOnceAsync();

// Sending a matching message will then trigger the registered handler and signal the event
// ...

// Assert expected message was received within timeout
var isReceived = isReceivedEvent.Wait(TimeSpan.FromSeconds(5));
isReceived.Should().BeTrue();
```

Example using `WhenAny()` and `VerifyCountAsync()` extensions:

```csharp
// Setup verification.
using var whenAllEvent = await serviceBusListenerMock
    .WhenAny()
    .VerifyCountAsync(expectedCount);

// Sending the expected number of messages will then trigger the registered handler and signal the event
// ...

// Assert number of messages was received within timeout
var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
allReceived.Should().BeTrue();
```
