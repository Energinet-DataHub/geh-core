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

It is the package consumers responsibility to implement the `IIntegrationEventProvider` and register it.

The guide below shows an **example** of an `IIntegrationEventProvider` implementation with necessary registrations and configuration.

Preparing a **Web App** project (similar can be done for a Function App project):

1) Install this NuGet package: `Energinet.DataHub.Core.Messaging.Communication`

1) Extend `Program.cs` with the following registrations

   ```csharp
   // Registration of dependencies
   builder.Services.AddServiceBusClientForApplication(builder.configuration);
   builder.Services.AddIntegrationEventsPublisher<IntegrationEventProvider>(builder.Configuration);
   ```

1) Implement a `IIntegrationEventProvider`.

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

Subscribing functionality is responsible for receiving and relaying `IntegrationEvents` to an `IIntegrationEventHandler`. Simply inject `ISubscriber` and call the `HandleAsync` method, which will then call the `IIntegrationEventHandler` implementation.

It is the package consumers responsibility to implement the `IIntegrationEventHandler` and register it.

The subscribing functionality can be used in two ways:

- Using a `ServiceBusTrigger` function
- Using a hosted `BackgroundService`

The guide below shows an **example** of an `IIntegrationEventHandler` and `ServiceBusTrigger` implementation with necessary registrations and configuration.

Preparing a **Function App** project:

1) Install this NuGet package: `Energinet.DataHub.Core.Messaging.Communication`

1) Extend `Program.cs` with the following registrations

   The _descriptors_ are used to deserialize the event as well as filtering unwanted messages. In the example, we expect messages of type `ActorCreated` and `UserCreated`.

   ```csharp
   // Registration of dependencies
   services.AddSubscriber<IntegrationEventHandler>(new[]
   {
       ActorCreated.Descriptor,
       UserCreated.Descriptor,
   });
   ```

1) Implement an `IIntegrationEventHandler`.

   ```csharp
   // Example implementation
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

1) Implement a `ServiceBusTrigger`.

   When using a `ServiceBusTrigger` to handle integration events, the `ISubscriber` dependency needs to be injected into the function and called in the manner shown below.

   Notice how we configure the trigger to use an identity-based connection by configuring the _configuration section_ in which to find the ServiceBus namespace. The property in the section **must** be named `FullyQualifiedNamespace`. For details read _Identity-based connection_ under [ServiceBusTrigger](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cextensionv5&pivots=programming-language-csharp).

    ```csharp
    // Example implementation
    public sealed class ServiceBusFunction
    {
        private readonly ISubscriber _subscriber;

        public ServiceBusFunction(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        [Function("ServiceBusFunction")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.TopicName)}%",
                $"%{IntegrationEventsOptions.SectionName}:{nameof(IntegrationEventsOptions.SubscriptionName)}%",
                Connection = ServiceBusNamespaceOptions.SectionName)]
            byte[] message,
            FunctionContext context)
        {
            await _subscriber.HandleAsync(IntegrationEventServiceBusMessage.Create(message, context.BindingContext.BindingData!));
        }
    }
    ```

1) Perform configuration in application settings

   ```json
   {
     "IsEncrypted": false,
     "Values": {
       // ServiceBus namespace
       "ServiceBus__FullyQualifiedNamespace": "<namespace>",
       // Integration Events topic/subscription
       "IntegrationEvents__TopicName": "<topic>",
       "IntegrationEvents__SubscriptionName": "<subscription>"
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
