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

## Publishing

The publishing functionality is responsible for publishing integration events. It can be used manually or through a hosted service.
Regardless of which "mode" is used, the IIntegrationEventProvider implementation has to be implemented.

Below code shows an example of an IIntegrationEventProvider implementation as well as the registration.

```csharp
// IIntegrationEventProvider implementation
public sealed class IntegrationEventProvider : IIntegrationEventProvider
{
    public IntegrationEventProvider(DbContext dbContext)
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
            
            await _dbContext.SaveChangesAsync();
        }
    }
}

// Registration of dependencies
services.Configure<PublisherOptions>(builder.Configuration.GetSection(nameof(PublisherOptions)));
services.AddPublisher<IntegrationEventProvider>();
```

### Manual

When publishing manually, the above IIntegrationEventProvider implementation and registration is enough to start publishing integration events.
Simply inject IPublisher and call the PublishAsync method, which will then call the IIntegrationEventProvider implementation, and dispatch the returned integration events.

### BackgroundService

When using the hosted BackgroundService, in addition to the registration of the IIntegrationEventProvider implementation shown above, the below code, registering the worker, is also needed.

```csharp
services.Configure<PublisherWorkerOptions>(builder.Configuration.GetSection(nameof(PublisherWorkerOptions)));
services.AddPublisherWorker();
```

## Subscribing

Subscribing functionality is responsible for receiving and relaying IntegrationEvents to an IIntegrationEventHandler implementation which, in the same manner as IIntegrationEventProvider, is the responsibility of the package consumer.
The subscribing functionality can be used in two ways: using a ServiceBusTrigger function or using a hosted BackgroundService.
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
Regardless of whether a ServiceBusTrigger or the hosted service is used, the IIntegrationEventHandler implementation needs to be registered as a dependency using the code below.
The descriptors are used to deserialize the event as well as filtering unwanted messages.

```csharp
services.AddSubscriber<IntegrationEventHandler>(new[]
{
    ActorCreated.Descriptor,
    UserCreated.Descriptor,
});
```

In order to deserialize protobuf messages, the package needs to know the descriptors of expected messages. In the example above, we expect messages of type ActorCreated and UserCreated.

### ServiceBusTrigger

When using a ServiceBusTrigger to handle integration events, the ISubscriber dependency needs to be injected into the function and called in the manner shown below.

```csharp
// MessageBusTrigger function
public sealed class ServiceBusFunction
{
    private readonly ISubscriber _subscriber;

    public ServiceBusFunction(ISubscriber subscriber)
    {
        _subscriber = subscriber;
    }

    [Function("ServiceBusFunction")]
    public async Task RunAsync(
        [ServiceBusTrigger("topic-...", "subscription-...", Connection = "ConnectionString")]
        byte[] message,
        FunctionContext context)
    {
        await _subscriber.HandleAsync(IntegrationEventServiceBusMessage.Create(message, context.BindingContext.BindingData!));
    }
}
```

### BackgroundService

When used as a hosted BackgroundService, in addition to the registration of the IIntegrationEventHandler implementation shown above, the below code, registering the worker, is also needed.

```csharp
services.Configure<SubscriberWorkerOptions>(builder.Configuration.GetSection(nameof(SubscriberWorkerOptions)));
services.AddSubscriberWorker();
```
