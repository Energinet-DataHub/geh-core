# Messaging

This package is intended for communication between different domains using ServiceBus.
The package ensures reliable message processing and delivery.

The package is still work in progress.

## Usage of outbox

Below code shows an example of an IItegrationEventProvider implementation and how to register the dependencies.
End result will be a hosted worker polling the IntegrationEventProvider for new integration events and dispatching them
via ServiceBus.

```csharp
// Implement IIntegrationEventProvider to provide integration events
public sealed class IntegrationEventProvider : IIntegrationEventProvider
{
    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        yield return new IntegrationEvent(
            Guid.NewGuid(),
            nameof(UserCreated),
            1,
            new UserCreated
            {
                FirstName = "John",
                LastName = "Doe"
            });
        // make sure to commit changes, after yield, as the event has now been dispatched
    }
}

// Registration of dependencies
services.AddOutboxWorker<IntegrationEventProvider>(_ => new OutboxWorkerSettings
{
    ServiceBusIntegrationEventWriteConnectionString = "Endpoint=sb://...",
    IntegrationEventTopicName = "topic-...",
    HostedServiceExecutionDelayMs = 1000,
});
```

UserCreated is a protobuf message defined in the same project as the IntegrationEventProvider.

## Usage of inbox

Inbox functionality can be used in two ways: using a ServiceBusTrigger function or using a hosted BackgroundService.
In both cases an IntegrationEventHandler implementation is needed to handle the integration events. An example of such
an implementation is shown below.

```csharp
public sealed class IntegrationEventHandler : IIntegrationEventHandler
{
    public bool ShouldHandle(string eventName)
    {
        return 
            eventName is
                nameof(ActorCreated) or
                nameof(UserCreated);
    }

    public async Task HandleAsync(IntegrationEvent integrationEvent)
    {
        switch (integrationEvent.Message)
        {
            case ActorCreated actorCreated:
                // handle actorCreated
                break;
            case UserCreated userCreated:
                // handle userCreated
                break;
        }
    }
}
```

In both cases the IIntegrationEventHandler implementation needs to be registered as a dependency using the below code.

```csharp
services.AddInbox<IntegrationEventHandler>(new[]
{
    ActorCreated.Descriptor,
    UserCreated.Descriptor,
});
```

### ServiceBusTrigger function

When using a ServiceBusTrigger function to handle integration events, the IInbox dependency needs to be injected into the function and called in the manner shown below.
The IInbox ensures deserialization of the protobuf message and calls the IIntegrationEventHandler implementation if it meets the criteria defined in the ShouldHandle method.

```csharp
// MessageBusTrigger function
public sealed class ServiceBusFunction
{
    private readonly IInbox _inbox;

    public ServiceBusFunction(IInbox inbox)
    {
        _inbox = inbox;
    }

    [Function("ServiceBusFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("topic-...", "subscription-...", Connection = "ConnectionString")]
        byte[] message,
        FunctionContext context)
    {
        await _inbox.HandleAsync(RawServiceBusMessage.Create(message, context.BindingContext.BindingData!));
    }
}
```

### BackgroundService

When used as a hosted BackgroundService, in addition to the registration of the IIntegrationEventHandler implementation shown above, the below code registering the worker, also needs to be run.

```csharp
services
    .AddInboxWorker(_ => new InboxWorkerSettings
    {
        ServiceBusConnectionString = "Endpoint=sb://...",
        TopicName = "topic-...",
        SubscriptionName = "subscription-...",
        HostedServiceExecutionDelayMs = 1000,
        MaxMessageDeliveryCount = 1000
    });

```
