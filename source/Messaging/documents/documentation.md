# Messaging

Documentation of the NuGet package bundle `Messaging`.

The `Messaging` package bundle contains common functionality for Azure ServiceBus implemented as dependency injection extensions, extensibility types etc.

Using the package bundle enables an easy opt-in/opt-out pattern of Azure ServiceBus related services during startup, for a typical DataHub application.

## Overview

<!-- TOC -->
- [Following best practices](#following-best-practices)
- [Integration Events communication](#integration-events-communication)
    - [Publishing](#publishing)
    - [Subscribing](#subscribing)
        - [ServiceBusTrigger](#servicebustrigger)
- [Health checks](#health-checks)
<!-- TOC -->

## Following best practices

**Important:** If the implemented extensions or types dosn't support what we need, and we have to implement any additional extensions or types, then be sure to follow the same best practices for any implementation.

The implemented extensions ensures we follow best practices documented by Microsoft:

- Use token-based authentication rather than connection strings
- Reuse factories and clients

The implemented extensions also builds upon Azure SDK extensions. By using the Azure SDK extensions for dependency injection we get the following benefits:

- Singleton instances
    - Lazy creation of instances
    - Cached instances
- Since instances are registered to the DI container we don't have to manually dispose them

For details, read the following Microsoft documentation:

- [Recommended app authentication approach](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/?tabs=command-line#recommended-app-authentication-approach)
- [Reusing factories and clients](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-performance-improvements?tabs=net-standard-sdk-2#reusing-factories-and-clients)
- [Register clients and subclients](https://learn.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection?tabs=host-builder#register-clients-and-subclients)

## Integration Events communication

A main part of this package is intended for handling integration events communication between different subsystems using Azure ServiceBus. For that part the types operates on the type `IntegrationEvent`, which wraps a protobuf message.

The `IntegrationEvent` is defined as follows:

```csharp
public record IntegrationEvent(
    Guid EventIdentification,
    string EventName,
    int EventMinorVersion,
    IMessage Message);
```

### Publishing

The publishing functionality is responsible for publishing integration events. Simply inject `IPublisher` and call the `PublishAsync` method, which will then call the `IIntegrationEventProvider` implementation, and dispatch the returned integration events.

For this to work the `IIntegrationEventProvider` interface has to be implemented. The guide below shows an example of an `IIntegrationEventProvider` implementation, as well as the necessary registrations and configuration.

Preparing a Web App project (similar can be done for an Azure Function application):

1) Install this NuGet package: `Energinet.DataHub.Core.Messaging.Communication`

1) Extend `Program.cs` with the following registrations

   ```csharp
   // Registration of dependencies
   builder.Services.AddServiceBusClientForApplication(builder.configuration);
   builder.Services.AddIntegrationEventsPublisher<IntegrationEventProvider>(builder.Configuration);
   ```

1) Implement `IIntegrationEventProvider` accordingly. The following is an example.

   ```csharp
   // Example implementation
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
   ```

1) Perform configuration in application settings

   ```json
   {
     // ServiceBus namespace
     "ServiceBus": {
       "FullyQualifiedNamespace": "<namespace>",
     },
     // Integration Events topic/subscription
     "IntegrationEvents": {
       "TopicName": "<topic>",
       "SubscriptionName": "<subscription>",
     }
   }
   ```

### Subscribing

Subscribing functionality is responsible for receiving and relaying `IntegrationEvents` to an `IIntegrationEventHandler` implementation which, in the same manner as `IIntegrationEventProvider`, is the responsibility of the package consumer.

The subscribing functionality can be used in two ways: using a `ServiceBusTrigger` function or using a hosted `BackgroundService`.
In both cases the `IIntegrationEventHandler` implementation is needed. An example of an `IIntegrationEventHandler` implementation is shown below.

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

Regardless of whether a `ServiceBusTrigger` or the hosted service is used, the `IIntegrationEventHandler` implementation needs to be registered as a dependency using the code below.

```csharp
services.AddSubscriber<IntegrationEventHandler>(new[]
{
    ActorCreated.Descriptor,
    UserCreated.Descriptor,
});
```

The descriptors are used to deserialize the event as well as filtering unwanted messages. In the example above, we expect messages of type `ActorCreated` and `UserCreated`.

#### ServiceBusTrigger

When using a ServiceBusTrigger to handle integration events, the `ISubscriber` dependency needs to be injected into the function and called in the manner shown below.

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

```csharp
services
    .AddHealthChecks()
    .AddServiceBusQueueDeadLetter(
        sp => sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value.FullyQualifiedNamespace,
        sp => sp.GetRequiredService<IOptions<IntegrationEventsOptions>>().Value.QueueName,
        _ => new DefaultAzureCredential(),
        "HealthCheckName",
        [HealthChecksConstants.StatusHealthCheckTag]);
```

The usage of the `StatusHealthCheckTag` from `App.Common.Diagnostics.HealthChecks` is optional but highly recommended.
It denotes that the health check should not block deployments if it fails.

Health checks can be added both with connection strings and fully qualified namespaces.
Note, however, that using connection strings is already an obsolete feature as we are pushing towards AIM on the service bus.

The health check is meant for monitoring the dead-letter queue of a ServiceBus subscription to a particular topic.
The health check will return unhealthy if there are any messages in the dead-letter queue.
