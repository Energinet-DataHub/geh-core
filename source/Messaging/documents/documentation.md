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

The publishing functionality is responsible for publishing integration events. The IIntegrationEventProvider interface has to be implemented.

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

When publishing, the above IIntegrationEventProvider interface and registration is enough to start publishing integration events.
Simply inject IPublisher and call the PublishAsync method, which will then call the IIntegrationEventProvider implementation, and dispatch the returned integration events.

## Subscribing

Subscribing functionality is responsible for receiving and relaying IntegrationEvents to an IIntegrationEventHandler implementation which, in the same manner as IIntegrationEventProvider, is the responsibility of the package consumer.
The subscribing functionality can be used in two ways: using a ServiceBusTrigger function or using a hosted BackgroundService.
In both cases the IIntegrationEventHandler implementation is needed. An example of an IIIntegrationEventHandler implementation is shown below.

```csharp
public sealed class IntegrationEventHandler : IIntegrationEventHandler
{
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

```csharp
services.AddSubscriber<IntegrationEventHandler>(new[]
{
    ActorCreated.Descriptor,
    UserCreated.Descriptor,
});
```

The descriptors are used to deserialize the event as well as filtering unwanted messages. In the example above, we expect messages of type ActorCreated and UserCreated.

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

## Health checks

The package provides an opt-in dead-letter health check, which can be registered using
`ServiceBusHealthCheckBuilderExtensions` as shown below:

```csharp
services
    .AddHealthChecks()
    .AddServiceBusTopicSubscriptionDeadLetter(
        sp => sp.GetRequiredService<IOptions<ServiceBusNamespaceOptions>>().Value.ConnectionString,
        sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.TopicName,
        sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.SubscriptionName,
        "HealthCheckName",
        [HealthChecksConstants.StatusHealthCheckTag]);
```

The usage of the `StatusHealthCheckTag` from `App.Common.Diagnostics.HealthChecks` is optional but highly recommended.
It denotes that the health check should not block deployments if it fails.

The health check is meant for monitoring the dead-letter queue of a ServiceBus subscription to a particular topic.
The health check will return unhealthy if there are any messages in the dead-letter queue.
