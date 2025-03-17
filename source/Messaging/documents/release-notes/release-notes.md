# Messaging Release notes

## Version 6.1.4

- Update tj-actions to v46.0.1
- No functional change.

## Version 6.1.3

- Update .github referencess to v14
- No functional change.

## Version 6.1.2

- Bumped various NuGet packages to the latest versions.
- No functional changes.

## Version 6.1.1

- Bumped various NuGet packages to the latest versions.
- No functional changes.

## Version 6.1.0

- Deleted unused options `SubscriberWorkerOptions`.
- Added dependency injection extensions `ServiceBusExtensions.AddDeadLetterHandlerForIsolatedWorker`.
- Mark method `public static IntegrationEventServiceBusMessage Create(byte[] message, IReadOnlyDictionary<string, object> bindingData)` as obsolete.

## Version 6.0.0

- Added dependency injection extensions `ServiceBusExtensions.AddServiceBusClientForApplication` and related options `ServiceBusNamespaceOptions`. This registration ensures we use identity access management (IAM) for the ServiceBus namespace.
- Added dependency injection extensions `ServiceBusExtensions.AddIntegrationEventsPublisher` and related options `IntegrationEventsOptions`.
- Mark dependency injection extensions `Registration.AddPublisher` and certain related types as obsolete.
- Implemented internal class `IntegrationEventsPublisher` as a substitute for the obsolete `Publisher`.
- Exposed types `IServiceBusMessageFactory` and `ServiceBusMessageFactory` to support extensibility and usage from other publisher implementations.
- Removed dependency injection extension `Registration.AddSubscriberWorker` and deleted related internal types.

## Version 5.1.0

- Added opt-in dead-letter health check `ServiceBusTopicSubscriptionDeadLetterHealthCheck`
- Added opt-in dead-letter health check `ServiceBusQueueDeadLetterHealthCheck`
- New health checks can be registered using `ServiceBusHealthCheckBuilderExtensions`

## Version 5.0.1

- No functional change

## Version 5.0.0

- Removed types `PublisherTrigger` and `PublisherWorkerOptions`
- Removed DI extension `AddPublisherWorker`
- Upgrade NuGet packages

## Version 4.0.0

- Upgrade to .NET 8
- Upgrade nuget packages
- No functional changes

## Version 3.3.1

- No functional change

## Version 3.3.0

- Extend `PublisherOptions` and `SubscriberWorkerOptions` with `TransportType` to allow consumers to decide the transport used when communicating with Service Bus.

## Version 3.2.1

- No functional change

## Version 3.2.0

- Support for the `ServiceBusReceivedMessage` type when creating a new `IntegrationEventServiceBusMessage`

## Version 3.1.1

- No functional change.

## Version 3.1.0

- Support for receiving messages from Service Bus using a background service or Service Bus trigger.
- Support for publishing messages ad hoc, for when a background service is not suitable.

## Version 3.0.0

- New implementation for sending messages via Service Bus using a background service.
- Includes extension for registration of needed dependencies.
- This is a breaking change, as the old implementation has been removed.

## Version 2.1.9

- No functional change.

## Version 2.1.8

- No functional change.

## Version 2.1.7

- No functional change.

## Version 2.1.6

- Add testresults to CI report

## Version 2.1.5

- No functional change.

## Version 2.1.4

- Bump version as part of pipeline change.

## Version 2.1.3

- Bumped patch version as pipeline file was updated.

## Version 2.1.2

- Updated packages.

## Version 2.1.1

- Bumped patch version as pipeline file was updated.

## Version 2.1.0

- Use default .NET Core SDK version pre-installed on Github Runner when running CI workflow

## Version 2.0.0

- Upgrade from .NET 5 to .NET 6

## Version 1.1.7

- Bumped patch version as pipeline file was updated.

## Version 1.1.6

- Bumped patch version as pipeline file was updated.

## Version 1.1.5

- Bumped patch version as pipeline file was updated.

## Version 1.1.4

- Bumped patch version as pipeline file was updated.

## Version 1.1.1-1.1.3

- Updated SchemaValidation package.

## Version 1.1.0

- Added SchemaValidatingMessageDeserializer.
- Added SchemaValidatedInboundMessage<TInboundMessage>.

## Version 1.0.0

- Initial release.
