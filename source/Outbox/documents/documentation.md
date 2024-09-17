# Documentation

This outbox bundle exists of 2 .NET NuGet packages:

- `Energinet.DataHub.Core.Outbox.Abstractions` contains the interfaces/abstractions needed to add messages to the outbox.
- `Energinet.DataHub.Core.Outbox` contains the outbox processor needed to process/publish outbox messages.

These 2 packages are needed to implement an outbox pattern in a .NET application, along with the dependencies described
in a section below.

## The outbox pattern

The Outbox module is a pattern for handling messages in a distributed system. The outbox pattern is used to ensure that
messages are sent at least once, even if the system crashes or the message is lost. This is achieved by storing the
message in a database (the outbox) in the same transaction as the actual business logic, and then processing 
the message in a separate process, which handles retries in case the external system is unavailable.

An example of the outbox pattern can be found at [the outbox pattern](https://www.kamilgrzybek.com/blog/posts/the-outbox-pattern)
(also referenced by Microsoft at
[asynchronous-message-based-communication](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication))

Using the outbox guarantees [At-least-once Delivery](https://www.cloudcomputingpatterns.org/at_least_once_delivery/) since 
messages are retried in case of failure. This should be used in combination with
[Idempotent Processor](https://www.cloudcomputingpatterns.org/idempotent_processor/) and
[Exactly-once Delivery](https://www.cloudcomputingpatterns.org/exactly_once_delivery/) patterns to ensures that
the received messages are only processed once.

## Getting started

This section describes how to get started using the outbox pattern in a .NET application. It consists of 
the following topics and examples:

1. Required dependencies.
    - Including setting up the `DbContext` / SQL table required by the outbox implementation.
2. Adding a new message to the outbox.
3. Publishing Outbox messages.

### Required dependencies

The following dependencies are required to exist in the DI container to use the outbox package:
- `IClock` (NodaTime, should be registered using the DataHub app package)
- `ILogger` (Microsoft.Extensions.Logging, should be registered using the DataHub app package)
- A `DbContext` implementing the `IOutboxContext` interface (see below)

**Outbox database context**

To use the `Outbox` package you first need to have an `DbContext` implementing the `IOutboxContext` interface, that
also applies the `OutboxEntityConfiguration` to the model builder (the `DbContext` must be registered in the DI container).

The `IOutboxContext` interface is used to provide the `Outbox` module with the necessary database context to store the
outbox messages. It is found in the `Energinet.DataHub.Core.Outbox` NuGet package.

**IOutboxContext example** (`MyApplicationDbContext`):
```csharp
public class MyApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext, IOutboxContext
{
    public MyApplicationDbContext(DbContextOptions<MyApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OutboxMessage> Outbox { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The outbox entity configuration must be added to the model builder to correctly configure the outbox table.
        modelBuilder.ApplyConfiguration(new OutboxEntityConfiguration());
    }
}
```

A script for creating the outbox table in SQL should be added to the database migration scripts, looking like this:
```sql
CREATE TABLE [dbo].[Outbox]
(
    [Id]                UNIQUEIDENTIFIER NOT NULL,
    [RecordId]          INT IDENTITY (1,1) NOT NULL,

    -- ROWVERSION makes Entity Framework throw an exception if trying to update a row which has already been updated (concurrency conflict)
    -- https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations
    [RowVersion]        ROWVERSION NOT NULL,
    
    [Type]              NVARCHAR(255) NOT NULL,
    [Payload]           NVARCHAR(MAX) NOT NULL,
    [CreatedAt]         DATETIME2 NOT NULL,
    [ProcessingAt]      DATETIME2 NULL,
    [PublishedAt]       DATETIME2 NULL,
    [FailedAt]          DATETIME2 NULL,
    [ErrorMessage]      NVARCHAR(MAX) NULL,
    [ErrorCount]        INT NOT NULL,

    CONSTRAINT [PK_Outbox]            PRIMARY KEY NONCLUSTERED ([Id]),
    
    -- A UNIQUE CLUSTERED constraint on an INT IDENTITY column optimizes the performance of the outbox table
    -- by ordering indexes by the sequential RecordId column instead of the UNIQUEIDENTIFIER  primary key(which is random).
    CONSTRAINT [UX_Outbox_RecordId]   UNIQUE CLUSTERED ([RecordId] ASC),
)
GO

-- The index used for finding up messages to process in the Outbox table
CREATE INDEX [IX_Outbox_PublishedAt_FailedAt_ProcessingAt_CreatedAt]
    ON [dbo].[Outbox] ([PublishedAt], [FailedAt], [ProcessingAt], [CreatedAt])
    INCLUDE ([Id])
GO
```

### Adding a new message to the outbox

To add new outbox messages to the outbox, you must use the `IOutboxClient` interface from 
the `Energinet.DataHub.Core.Outbox.Abstractions`NuGet package. Register the required dependencies
and the `IOutboxClient` in your application:

```csharp
IServiceCollection services = new ServiceCollection();

/// => Add required dependencies like IClock, ILogger, DbContext etc. descriped in the earlier section
... Add required dependencies
    
// => Add services needed to add a message to the outbox
// These are the only services needed if the application is only adding messages to the outbox, and the
// outbox processing is running in a different application.
services.AddOutboxClient<MyApplicationDbContext>();
```

The `MyApplicationDbContext` is the `DbContext` implementing the `IOutboxContext` interface, 
described in the "required dependencies" section.

#### Implementing an outbox message

To add a new outbox message to the outbox, you must also create an outbox message implementation. 
Add new class that implements the `IOutboxMessage<TPayload>` interface (see example below).

**IOutboxMessage (UserCreatedEmailOutboxMessageV1) example:**
```csharp
public record UserCreatedEmailOutboxMessageV1
    : IOutboxMessage<UserCreatedEmailOutboxMessageV1Payload>
{
    public const string OutboxMessageType = "UserCreatedEmailOutboxMessageV1";

    public UserCreatedEmailOutboxMessageV1(Guid id, string email)
    {
        Payload = new UserCreatedEmailOutboxMessageV1Payload(id, email);
    }

    public string Type => OutboxMessageType;

    public UserCreatedEmailOutboxMessageV1Payload Payload { get; }

    public Task<string> SerializeAsync()
    {
        // => Serialize the payload to a string, which is deserialized in the appropriate IOutboxPublisher.
        // In a real world implementation this should use the ISerializer from the DataHub Serializer package.
        return Task.FromResult(JsonSerializer.Serialize(Payload));
    }
}

public record UserCreatedEmailOutboxMessageV1Payload(Guid Id, string Email);
```

#### Adding an outbox message to the outbox

To add an outbox message to the outbox, you must perform the following:
 
1. Inject the `IOutboxClient` interface into your service.
2. Create a new instance of an outbox message implementation (see `UserCreatedEmailOutboxMessageV1`example above).
3. Add the outbox message to the outbox using the `AddToOutboxAsync` method on the `IOutboxClient`.
4. Save changes to the database context.

When using an outbox pattern, it **is very important** that saving the changes on the `DbContext` happens in
the same transaction as other business logic, since that is how the outbox pattern guarantees that the
message will be sent if the transaction is committed successfully (see [At-least-once Delivery pattern](https://www.cloudcomputingpatterns.org/at_least_once_delivery/), which is/should be used with [Idempotent Processor](https://www.cloudcomputingpatterns.org/idempotent_processor/)).

```csharp
public class CreateUserService(IOutboxClient outboxClient, MyApplicationDbContext dbContext)
{
    private readonly IOutboxClient _outboxClient = outboxClient;
    private readonly MyApplicationDbContext _dbContext = dbContext;

    public async Task CreateAsync(string email)
    {
        // Actually create the user in the database here
        var user = CreateUser(email);
    
        // Add the outbox message to the outbox
        var sendEmailOutboxMessage = new UserCreatedEmailOutboxMessageV1(user.Id, user.Email);
        await _outboxClient.AddToOutboxAsync(sendEmailOutboxMessage)
            .ConfigureAwait(false);

        // Save the user and the outbox message in the same transaction
        await _dbContext.SaveChangesAsync()
            .ConfigureAwait(false);
    }
}
```

### Publishing Outbox messages

Since the outbox message is stored in the database, publishing the outbox message can happen in any application that
has access to the same database. 


#### Implementing an outbox message publisher

To be able to publish the `UserCreatedEmailOutboxMessageV1` example from above, a corresponding `IOutboxPublisher`
must be implemented.

**Example UserCreatedEmailOutboxMessagePublisher:**
```csharp
public class UserCreatedEmailOutboxMessagePublisher : IOutboxPublisher
{
    public bool CanPublish(string type) => type.Equals(UserCreatedEmailOutboxMessageV1.OutboxMessageType);

    public Task PublishAsync(string serializedPayload)
    {
        var payload = JsonSerializer.Deserialize<UserCreatedEmailOutboxMessageV1Payload>(serializedPayload)
                      ?? throw new InvalidOperationException($"Failed to deserialize payload of type {nameof(UserCreatedEmailOutboxMessageV1Payload)}");

        Console.WriteLine($"Payload id={payload.Id}, email={payload.Email}");

        // Implementation of publishing the message, e.g. sending an email, sending a http request, adding to a service bus etc.
        return Task.CompletedTask;
    }
}
```

#### Publishing outbox messages

The `IOutboxProcessor` from the `Energinet.DataHub.Core.Outbox` NuGet package interface is used to process and publish
outbox messages. The following must be performed in the application that should process the outbox messages:

- Install the `Energinet.DataHub.Core.Outbox` NuGet package.
- Add required dependencies (`IClock`, `ILogger`, `DbContext`/`IOutboxContext` etc.) described in an earlier section.
- Register the required `IOutboxPublisher` implementations (`UserCreatedEmailOutboxMessagePublisher` in this example) in the DI container.
- Register the `IOutboxProcessor` using the `AddOutboxProcessor<TContext>()` extension in the DI container.

```csharp
IServiceCollection services = new ServiceCollection();

/// => Add required dependencies like IClock, ILogger, DbContext etc. descriped in the earlier section
... Add required dependencies

// => Add services needed for processing outbox messages (ie. publishing the messages)
// These are the only services needed if the application is only processing messages from the outbox, not adding
// new messages to the outbox
services.AddTransient<IOutboxPublisher, UserCreatedEmailOutboxMessagePublisher>(); // The outbox message publishers specific to the application
services.AddOutboxProcessor<MyApplicationDbContext>(); // The outbox processor that should run periodically in a background worker or a timer trigger
```

The `IOutboxProcessor` will process the outbox messages in the database and publish them using
the registered `IOutboxPublisher` implementations. It should be called periodically from a background worker
or a timer trigger. Below is an example of how to use the `IOutboxProcessor` in a timer trigger,
processing the outbox messages every 10 seconds.

**TimerTrigger example:**
```csharp
public class OutboxPublisher(IOutboxProcessor outboxProcessor)
{
    private readonly IOutboxProcessor _outboxProcessor = outboxProcessor;

    [Function(nameof(OutboxPublisher))]
    public Task PublishOutboxAsync(
        [TimerTrigger("*/10 * * * * *")] TimerInfo timerTimerInfo,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        return _outboxProcessor.ProcessOutboxAsync();
    }
}
```

The `_outboxProcessor.ProcessOutboxAsync()` will process all current outbox messages in the database 
and publish them using the registered `IOutboxPublisher` implementations. Using the default values, it will
maximum try to process 1000 messages per run, which can be overriden by using the optional `limit` parameter
from the `IOutboxProcessor.ProcessOutboxAsync` method.

```csharp
/// <summary>
/// Outbox processor responsible for publishing outbox messages that hasn't been processed yet.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Processes the outbox, publishing outbox messages that hasn't been processed yet.
    /// </summary>
    /// <param name="limit">The limit of messages to process each time the method is called.</param>
    /// <param name="cancellationToken"></param>
    Task ProcessOutboxAsync(int limit = 1000, CancellationToken? cancellationToken = null);
}
```

If running the `IOutboxProcessor` in a background worker, you must handle calling the `ProcessOutboxAsync` method
periodically. Make sure that each run of the `ProcessOutboxAsync` method is in a separate dependency injection scope.

## Deleting old outbox messages

Remember to delete old outbox messages from the `Outbox` table (where `PublishedAt` is not null) to avoid
the Outbox table continuing to growing indefinitely. Implementing this is currently not part of the `Outbox` 
package, and should be handled individually in each application.

## Error handling and retries

The `IOutboxProcessor` implementation will handle retries and error handling for the outbox messages. 
If an outbox message is not successfully processed, the `ErrorCount` will be incremented and
the `FailedAt` and `ErrorMessage` columns will be updated. The `IOutboxProcessor` will retry processing
the message with an increasing delay based on how many times it has failed.

When the message has failed to be sent 10 times, the message will retry once every day, until it has been
sent successfully. This is the maximum time between retries.

## Parallel processing

The `IOutboxProcessor` implementation can be run in parallel, using the `RowVersion` column in the `Outbox` table
to ensure that parallel running processors cannot process the same message. The current `IOutboxProcessor`
implementation is however not intended nor optimized for running in parallel.
