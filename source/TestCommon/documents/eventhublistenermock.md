# EventHubListenerMock

The `EventHubListenerMock` makes it easy to test and verify code that sends events to an Azure Event Hub.

The `EventHubListenerMock` connects to an actual Azure Event Hub instance. Using a fluent API developers can register event handlers for received events, similar to setting up verifications on a mock. So basically developers tells what event or events they expect to receive, and what should happen when an event is received.

> For usage, see `EventHubListenerMockTests` or [Aggregations](https://github.com/Energinet-DataHub/geh-aggregations) repository/domain.

## Replay of events

Already received events will be "replayed" on any new event handler that is registered.

This allows us to register event handlers and get notified correctly, even after the listener has started receiving events.

## Operations

### `InitializeAsync()`

Prepare the configured storage container for checkpointing, and start receiving events from the configured event hub.

### `When()` and `DoAsync()`

At the lowest level we can use `When()` to setup a filter based on any event data property. If an incomming event matches the filter, the action registered with `DoAsync()` is executed.

```csharp
await eventHubListenerMock
    .When(receivedEvent =>
        receivedEvent.MessageId == expectedMessageId)
    .DoAsync(_ =>
    {
        // Do something here...
        return Task.CompletedTask;
    });
```

#### Extensions

On top of `When()` and `DoAsync()` we have implemented several extensions to make the usage simpler.

When extensions:

- `WhenAny()`: When any event arrive.
- `WhenMessageId(expectedMessageId)`: When an event arrives with expected message id.
- `WhenCorrelationId(expectedCorrelationId)`: When an event arrives with expected correlation id.

Do extensions:

- `VerifyOnceAsync()`: Returns a reset event that can be waited, and that will be signaled when an event matching the *When* filter arrives.
- `VerifyOnceAsync(eventHandler)`: Overloaded version that will also call `eventHandler`.
- `VerifyCountAsync(expectedCount)`: Returns a countdown event that can be waited, and that will be signaled when the expected number of events matching the *When* filter has arrived.
- `VerifyCountAsync(expectedCount, eventHandler)`: Overloaded version that will also call `eventHandler`.

### `ReceivedEvents`

Readonly collection that contains the events that has arrived.

### `Reset()`

Call reset to clear all handlers and already received events. Use this between tests.

## Examples

Prepare event hub listener mock:

```csharp
// Remember to Dispose() the mock to shutdown the listener and close connections.
await using var eventHubListenerMock = new EventHubListenerMock(eventHubConnectionString, eventHubName, storageConnectionString, blobContainerName, testDiagnosticsLogger);

// Initialize listener.
await eventHubListenerMock.InitializeAsync();
```

Example using `WhenMessageId()` and `VerifyOnceAsync()` extensions:

```csharp
// E.g. reset between tests by adding this to test class constructor.
eventHubListenerMock.Reset();

// Setup verification.
// Here we expect an event with message id to arrive.
using var isReceivedEvent = await eventHubListenerMock
    .WhenMessageId(expectedMessageId)
    .VerifyOnceAsync();

// Sending a matching event will then trigger the registered handler and signal the event
// ...

// Assert expected event was received within timeout
var isReceived = isReceivedEvent.Wait(TimeSpan.FromSeconds(5));
isReceived.Should().BeTrue();
```

Example using `WhenAny()` and `VerifyCountAsync()` extensions:

```csharp
// Setup verification.
using var whenAllEvent = await eventHubListenerMock
    .WhenAny()
    .VerifyCountAsync(expectedCount);

// Sending the expected number of events will then trigger the registered handler and signal the event
// ...

// Assert number of events was received within timeout
var allReceived = whenAllEvent.Wait(TimeSpan.FromSeconds(5));
allReceived.Should().BeTrue();
```
