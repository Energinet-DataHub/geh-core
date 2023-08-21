# Messaging

This package is intended for communication between different domains using ServiceBus.
The package ensures reliable message processing and delivery.

The package operates on the type IntegrationEvent, which wraps a protobuf message.

The IntegrationEvent is defined as follows:

```csharp
public record IntegrationEvent(
    Guid EventIdentification,
    string EventName,
    int EventMinorVersion,
    IMessage Message);
```

The package is still work in progress.

## Outbox

The outbox functionality is responsible for dispatching integration events to the ServiceBus.
It does this using a hosted worker that polls the IIntegrationEventProvider for new integration events.
The IIntegrationEventProvider implementation is the responsibility of the package consumer.

Below code shows an example of an IIntegrationEventProvider implementation and how to register the dependencies using the package provided extension.

```csharp
// IIntegrationEventProvider implementation
public sealed class IntegrationEventProvider : IIntegrationEventProvider
{
    public IntegrationEventProvider(OutboxDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async IAsyncEnumerable<IntegrationEvent> GetAsync()
    {
        var events = await _dbContext
            .Events
            .Where(x => x.DispatchedAt == null)
            .ToListAsync();
        
        foreach (var e in events)
        {
            yield return new IntegrationEvent(
                e.Id,
                e.EventName,
                e.Version,
                e.Payload);
            
            e.DispatchedAt = DateTime.UtcNow;
            
            _dbContext.Events.Update(e);
            await _dbContext.SaveChangesAsync();
        }
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

## Inbox

Inbox functionality is responsible for receiving and relaying IntegrationEvents to an IIntegrationEventHandler implementation which, in the same manner as IIntegrationEventProvider, is the responsibility of the package consumer.
The inbox functionality can be used in two ways: using a ServiceBusTrigger function or using a hosted BackgroundService.
In both cases the IIntegrationEventHandler implementation is needed. An example of an IIIntegrationEventHandler implementation is shown below. 

```csharp
public sealed class IntegrationEventHandler : IIntegrationEventHandler
{
    public bool ShouldHandle(string eventName)
    {
        return eventName is
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

The `ShouldHandle` function is used to determine if the IIntegrationEventHandler implementation should handle the IntegrationEvent.
This function is called by the package internals, and only if it returns true, the `HandleAsync` method is called.

Regardless of whether a ServiceBusTrigger or the hosted service is used, the IIntegrationEventHandler implementation needs to be registered as a dependency using the code below.

```csharp
services.AddInbox<IntegrationEventHandler>(new[]
{
    ActorCreated.Descriptor,
    UserCreated.Descriptor,
});
```

In order to deserialize protobuf messages, the package needs to know the descriptors of expected messages. In the example above, we expect messages of type ActorCreated and UserCreated.

### ServiceBusTrigger

When using a ServiceBusTrigger to handle integration events, the IInbox dependency needs to be injected into the function and called in the manner shown below.

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

When used as a hosted BackgroundService, in addition to the registration of the IIntegrationEventHandler implementation shown above, the below code, registering the worker, is also needed.

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
