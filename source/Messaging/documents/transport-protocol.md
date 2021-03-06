# Message transport

When a host needs to send or receive data from another host, the `GreenEnergyHub.Message.Transport` namespace provides an abstraction.

## Sequence diagram for sending and receiving data

When sending data to another process the conceptual flow is:
[![Send sequence diagram](https://mermaid.ink/img/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSGFuZGxlclxuICAgIHBhcnRpY2lwYW50IE1lc3NhZ2VEaXNwYXRjaGVyXG4gICAgcGFydGljaXBhbnQgU2VyaWFsaXplclxuICAgIHBhcnRpY2lwYW50IE1hcHBlclxuICAgIHBhcnRpY2lwYW50IENoYW5uZWxcbiAgICBIYW5kbGVyLT4-TWVzc2FnZURpc3BhdGNoZXI6IFNlbmQgSHViTWVzc2FnZVxuICAgIE1lc3NhZ2VEaXNwYXRjaGVyLT4-U2VyaWFsaXplcjpTZXJpYWxpemUgdG8gYnl0ZXNcbiAgICBTZXJpYWxpemVyLT4-TWFwcGVyOk1hcCBmcm9tIEh1Yk1lc3NhZ2UgdG8gRFRPXG4gICAgTWFwcGVyLS0-PlNlcmlhbGl6ZXI6RFRPXG4gICAgU2VyaWFsaXplci0tPj5NZXNzYWdlRGlzcGF0Y2hlcjpieXRlc1xuICAgIE1lc3NhZ2VEaXNwYXRjaGVyLT4-Q2hhbm5lbDpEaXNwYXRjaCBtZXNzYWdlIHNlcmlhbGl6ZWQgYXMgYnl0ZXNcbiAgICAgICAgTm90ZSBvdmVyIE1lc3NhZ2VEaXNwYXRjaGVyLENoYW5uZWw6IEJ1aWx0IGluIHJlc2lsaWVuY2UsIHRoaW5rIFBvbGx5XG4gICAgTWVzc2FnZURpc3BhdGNoZXItLT4-SGFuZGxlcjpIdWJNZXNzYWdlIGlzIHNlbnQiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSGFuZGxlclxuICAgIHBhcnRpY2lwYW50IE1lc3NhZ2VEaXNwYXRjaGVyXG4gICAgcGFydGljaXBhbnQgU2VyaWFsaXplclxuICAgIHBhcnRpY2lwYW50IE1hcHBlclxuICAgIHBhcnRpY2lwYW50IENoYW5uZWxcbiAgICBIYW5kbGVyLT4-TWVzc2FnZURpc3BhdGNoZXI6IFNlbmQgSHViTWVzc2FnZVxuICAgIE1lc3NhZ2VEaXNwYXRjaGVyLT4-U2VyaWFsaXplcjpTZXJpYWxpemUgdG8gYnl0ZXNcbiAgICBTZXJpYWxpemVyLT4-TWFwcGVyOk1hcCBmcm9tIEh1Yk1lc3NhZ2UgdG8gRFRPXG4gICAgTWFwcGVyLS0-PlNlcmlhbGl6ZXI6RFRPXG4gICAgU2VyaWFsaXplci0tPj5NZXNzYWdlRGlzcGF0Y2hlcjpieXRlc1xuICAgIE1lc3NhZ2VEaXNwYXRjaGVyLT4-Q2hhbm5lbDpEaXNwYXRjaCBtZXNzYWdlIHNlcmlhbGl6ZWQgYXMgYnl0ZXNcbiAgICAgICAgTm90ZSBvdmVyIE1lc3NhZ2VEaXNwYXRjaGVyLENoYW5uZWw6IEJ1aWx0IGluIHJlc2lsaWVuY2UsIHRoaW5rIFBvbGx5XG4gICAgTWVzc2FnZURpc3BhdGNoZXItLT4-SGFuZGxlcjpIdWJNZXNzYWdlIGlzIHNlbnQiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)

[![Class diagram for sending data](https://mermaid.ink/img/eyJjb2RlIjoiY2xhc3NEaWFncmFtXG5cbiAgICBNZXNzYWdlRGVzZXJpYWxpemVyIDx8LS0gIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplclxuICAgIFxuICAgIE1lc3NhZ2VFeHRyYWN0b3IgKi0tIE1lc3NhZ2VEZXNlcmlhbGl6ZXJcblxuICAgIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciAqLS0gUHJvdG9idWZJbmJvdW5kTWFwcGVyRmFjdG9yeVxuICAgIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciAqLS0gUHJvdG9idWZQYXJzZXJcblxuICAgIFByb3RvYnVmSW5ib3VuZE1hcHBlckZhY3RvcnkgLi4-IFByb3RvYnVmSW5ib3VuZE1hcHBlclxuXG5cbiAgICBjbGFzcyBNZXNzYWdlRXh0cmFjdG9yIHtcbiAgICAgICAgK0V4dHJhY3RBc3luYyhieXRlW10pIElJbmJvdW5kTWVzc2FnZVxuICAgIH1cblxuICAgIGNsYXNzIE1lc3NhZ2VEZXNlcmlhbGl6ZXIge1xuICAgICAgICA8PGFic3RyYWN0Pj5cbiAgICAgICAgK0Zyb21CeXRlc0FzeW5jKGJ5dGVbXSkgSUluYm91bmRNZXNzYWdlXG4gICAgfVxuICAgIFxuICAgIGNsYXNzIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciB7XG4gICAgICAgIDw8c2VhbGVkPj5cbiAgICAgICAgK0Zyb21CeXRlc0FzeW5jKGJ5dGVbXSkgSUluYm91bmRNZXNzYWdlXG4gICAgfVxuXG4gICAgY2xhc3MgUHJvdG9idWZJbmJvdW5kTWFwcGVyRmFjdG9yeSB7XG4gICAgICAgICtHZXRNYXBwZXIoVHlwZSkgUHJvdG9idWZJbmJvdW5kTWFwcGVyXG4gICAgfVxuXG4gICAgY2xhc3MgUHJvdG9idWZQYXJzZXIge1xuICAgICAgICArUGFyc2UoYnl0ZVtdKSogSU1lc3NhZ2VcbiAgICB9XG5cbiAgICBjbGFzcyBQcm90b2J1ZkluYm91bmRNYXBwZXIge1xuICAgICAgICArQ29udmVydChJTWVzc2FnZSkqIElJbmJvdW5kTWVzc2FnZVxuICAgIH0iLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoiY2xhc3NEaWFncmFtXG5cbiAgICBNZXNzYWdlRGVzZXJpYWxpemVyIDx8LS0gIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplclxuICAgIFxuICAgIE1lc3NhZ2VFeHRyYWN0b3IgKi0tIE1lc3NhZ2VEZXNlcmlhbGl6ZXJcblxuICAgIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciAqLS0gUHJvdG9idWZJbmJvdW5kTWFwcGVyRmFjdG9yeVxuICAgIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciAqLS0gUHJvdG9idWZQYXJzZXJcblxuICAgIFByb3RvYnVmSW5ib3VuZE1hcHBlckZhY3RvcnkgLi4-IFByb3RvYnVmSW5ib3VuZE1hcHBlclxuXG5cbiAgICBjbGFzcyBNZXNzYWdlRXh0cmFjdG9yIHtcbiAgICAgICAgK0V4dHJhY3RBc3luYyhieXRlW10pIElJbmJvdW5kTWVzc2FnZVxuICAgIH1cblxuICAgIGNsYXNzIE1lc3NhZ2VEZXNlcmlhbGl6ZXIge1xuICAgICAgICA8PGFic3RyYWN0Pj5cbiAgICAgICAgK0Zyb21CeXRlc0FzeW5jKGJ5dGVbXSkgSUluYm91bmRNZXNzYWdlXG4gICAgfVxuICAgIFxuICAgIGNsYXNzIFByb3RvYnVmTWVzc2FnZURlc2VyaWFsaXplciB7XG4gICAgICAgIDw8c2VhbGVkPj5cbiAgICAgICAgK0Zyb21CeXRlc0FzeW5jKGJ5dGVbXSkgSUluYm91bmRNZXNzYWdlXG4gICAgfVxuXG4gICAgY2xhc3MgUHJvdG9idWZJbmJvdW5kTWFwcGVyRmFjdG9yeSB7XG4gICAgICAgICtHZXRNYXBwZXIoVHlwZSkgUHJvdG9idWZJbmJvdW5kTWFwcGVyXG4gICAgfVxuXG4gICAgY2xhc3MgUHJvdG9idWZQYXJzZXIge1xuICAgICAgICArUGFyc2UoYnl0ZVtdKSogSU1lc3NhZ2VcbiAgICB9XG5cbiAgICBjbGFzcyBQcm90b2J1ZkluYm91bmRNYXBwZXIge1xuICAgICAgICArQ29udmVydChJTWVzc2FnZSkqIElJbmJvdW5kTWVzc2FnZVxuICAgIH0iLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)

When a host process receives an input another flow begins to process the incoming data.

[![Receive sequence diagram](https://mermaid.ink/img/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSG9zdFxuICAgIHBhcnRpY2lwYW50IE1lc3NhZ2VFeHRyYWN0b3JcbiAgICBwYXJ0aWNpcGFudCBEZXNlcmlhbGl6ZXJcbiAgICBwYXJ0aWNpcGFudCBNYXBwZXJcbiAgICBwYXJ0aWNpcGFudCBIYW5kbGVyXG4gICAgSG9zdC0-Pk1lc3NhZ2VFeHRyYWN0b3I6RXh0cmFjdCBtZXNzYWdlIGZyb20gYnl0ZXNcbiAgICBNZXNzYWdlRXh0cmFjdG9yLT4-RGVzZXJpYWxpemVyOiBEZXNlcmlhbGl6ZSBieXRlc1xuICAgIERlc2VyaWFsaXplci0-Pk1hcHBlcjogTWFwIGZyb20gRFRPIHRvIEh1Yk1lc3NhZ2VcbiAgICBNYXBwZXItLT4-RGVzZXJpYWxpemVyOiBIdWJNZXNzYWdlXG4gICAgRGVzZXJpYWxpemVyLS0-Pk1lc3NhZ2VFeHRyYWN0b3I6IEh1Yk1lc3NhZ2VcbiAgICBNZXNzYWdlRXh0cmFjdG9yLS0-Pkhvc3Q6IEh1Yk1lc3NhZ2VcbiAgICBIb3N0LT4-SGFuZGxlcjogSGFuZGxlIG1lc3NhZ2UiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoic2VxdWVuY2VEaWFncmFtXG4gICAgcGFydGljaXBhbnQgSG9zdFxuICAgIHBhcnRpY2lwYW50IE1lc3NhZ2VFeHRyYWN0b3JcbiAgICBwYXJ0aWNpcGFudCBEZXNlcmlhbGl6ZXJcbiAgICBwYXJ0aWNpcGFudCBNYXBwZXJcbiAgICBwYXJ0aWNpcGFudCBIYW5kbGVyXG4gICAgSG9zdC0-Pk1lc3NhZ2VFeHRyYWN0b3I6RXh0cmFjdCBtZXNzYWdlIGZyb20gYnl0ZXNcbiAgICBNZXNzYWdlRXh0cmFjdG9yLT4-RGVzZXJpYWxpemVyOiBEZXNlcmlhbGl6ZSBieXRlc1xuICAgIERlc2VyaWFsaXplci0-Pk1hcHBlcjogTWFwIGZyb20gRFRPIHRvIEh1Yk1lc3NhZ2VcbiAgICBNYXBwZXItLT4-RGVzZXJpYWxpemVyOiBIdWJNZXNzYWdlXG4gICAgRGVzZXJpYWxpemVyLS0-Pk1lc3NhZ2VFeHRyYWN0b3I6IEh1Yk1lc3NhZ2VcbiAgICBNZXNzYWdlRXh0cmFjdG9yLS0-Pkhvc3Q6IEh1Yk1lc3NhZ2VcbiAgICBIb3N0LT4-SGFuZGxlcjogSGFuZGxlIG1lc3NhZ2UiLCJtZXJtYWlkIjp7InRoZW1lIjoiZGVmYXVsdCJ9LCJ1cGRhdGVFZGl0b3IiOmZhbHNlfQ)

[![Class diagram for receiving data](https://mermaid.ink/img/eyJjb2RlIjoiY2xhc3NEaWFncmFtXG5cbiAgICBNZXNzYWdlU2VyaWFsaXplciA8fC0tICBQcm90b2J1Zk1lc3NhZ2VTZXJpYWxpemVyXG4gICAgXG4gICAgTWVzc2FnZURpc3BhdGNoZXIgKi0tIE1lc3NhZ2VTZXJpYWxpemVyXG4gICAgTWVzc2FnZURpc3BhdGNoZXIgKi0tIENoYW5uZWxcblxuICAgIFByb3RvYnVmTWVzc2FnZVNlcmlhbGl6ZXIgKi0tIFByb3RvYnVmT3V0Ym91bmRNYXBwZXJGYWN0b3J5XG5cbiAgICBQcm90b2J1Zk91dGJvdW5kTWFwcGVyRmFjdG9yeSAuLj4gUHJvdG9idWZPdXRib3VuZE1hcHBlclxuXG5cbiAgICBjbGFzcyBNZXNzYWdlRGlzcGF0Y2hlciB7XG4gICAgICAgICtEaXNwYXRjaEFzeW5jKElPdXRib3VuZE1lc3NhZ2UpXG4gICAgfVxuXG4gICAgY2xhc3MgTWVzc2FnZVNlcmlhbGl6ZXIge1xuICAgICAgICA8PGFic3RyYWN0Pj5cbiAgICAgICAgK1RvQnl0ZXNBc3luYyhJT3V0Ym91bmRNZXNzYWdlKSBieXRlW11cbiAgICB9XG4gICAgXG4gICAgY2xhc3MgUHJvdG9idWZNZXNzYWdlU2VyaWFsaXplciB7XG4gICAgICAgIDw8c2VhbGVkPj5cbiAgICAgICAgK1RvQnl0ZXNBc3luYyhJT3V0Ym91bmRNZXNzYWdlKSBieXRlW11cbiAgICB9XG5cbiAgICBjbGFzcyBQcm90b2J1Zk91dGJvdW5kTWFwcGVyRmFjdG9yeSB7XG4gICAgICAgICtHZXRNYXBwZXIoSU91dGJvdW5kTWVzc2FnZSkgUHJvdG9idWZPdXRib3VuZE1hcHBlclxuICAgIH1cblxuICAgIGNsYXNzIFByb3RvYnVmT3V0Ym91bmRNYXBwZXIge1xuICAgICAgICArQ29udmVydChJT3V0Ym91bmRNZXNzYWdlKSogSU1lc3NhZ2VcbiAgICB9XG5cbiAgICBjbGFzcyBDaGFubmVsIHtcbiAgICAgICAgfldyaXRlVG9Bc3luYyhieXRlW10pXG4gICAgICAgICNXcml0ZUFzeW5jKGJ5dGVbXSkqXG4gICAgfSIsIm1lcm1haWQiOnsidGhlbWUiOiJkZWZhdWx0In0sInVwZGF0ZUVkaXRvciI6ZmFsc2V9)](https://mermaid-js.github.io/mermaid-live-editor/#/edit/eyJjb2RlIjoiY2xhc3NEaWFncmFtXG5cbiAgICBNZXNzYWdlU2VyaWFsaXplciA8fC0tICBQcm90b2J1Zk1lc3NhZ2VTZXJpYWxpemVyXG4gICAgXG4gICAgTWVzc2FnZURpc3BhdGNoZXIgKi0tIE1lc3NhZ2VTZXJpYWxpemVyXG4gICAgTWVzc2FnZURpc3BhdGNoZXIgKi0tIENoYW5uZWxcblxuICAgIFByb3RvYnVmTWVzc2FnZVNlcmlhbGl6ZXIgKi0tIFByb3RvYnVmT3V0Ym91bmRNYXBwZXJGYWN0b3J5XG5cbiAgICBQcm90b2J1Zk91dGJvdW5kTWFwcGVyRmFjdG9yeSAuLj4gUHJvdG9idWZPdXRib3VuZE1hcHBlclxuXG5cbiAgICBjbGFzcyBNZXNzYWdlRGlzcGF0Y2hlciB7XG4gICAgICAgICtEaXNwYXRjaEFzeW5jKElPdXRib3VuZE1lc3NhZ2UpXG4gICAgfVxuXG4gICAgY2xhc3MgTWVzc2FnZVNlcmlhbGl6ZXIge1xuICAgICAgICA8PGFic3RyYWN0Pj5cbiAgICAgICAgK1RvQnl0ZXNBc3luYyhJT3V0Ym91bmRNZXNzYWdlKSBieXRlW11cbiAgICB9XG4gICAgXG4gICAgY2xhc3MgUHJvdG9idWZNZXNzYWdlU2VyaWFsaXplciB7XG4gICAgICAgIDw8c2VhbGVkPj5cbiAgICAgICAgK1RvQnl0ZXNBc3luYyhJT3V0Ym91bmRNZXNzYWdlKSBieXRlW11cbiAgICB9XG5cbiAgICBjbGFzcyBQcm90b2J1Zk91dGJvdW5kTWFwcGVyRmFjdG9yeSB7XG4gICAgICAgICtHZXRNYXBwZXIoSU91dGJvdW5kTWVzc2FnZSkgUHJvdG9idWZPdXRib3VuZE1hcHBlclxuICAgIH1cblxuICAgIGNsYXNzIFByb3RvYnVmT3V0Ym91bmRNYXBwZXIge1xuICAgICAgICArQ29udmVydChJT3V0Ym91bmRNZXNzYWdlKSogSU1lc3NhZ2VcbiAgICB9XG5cbiAgICBjbGFzcyBDaGFubmVsIHtcbiAgICAgICAgfldyaXRlVG9Bc3luYyhieXRlW10pXG4gICAgICAgICNXcml0ZUFzeW5jKGJ5dGVbXSkqXG4gICAgfSIsIm1lcm1haWQiOnsidGhlbWUiOiJkZWZhdWx0In0sInVwZGF0ZUVkaXRvciI6ZmFsc2V9)

## Using Protocol Buffers for message encoding

Protocol Buffers provides a uniform way to describe contracts for data objects. It is platform and language independent. The contract, proto-file, is compiled to the destination language C#, Python, Java, etc.

For details about Protocol Buffers, please visit the developer site: [Protocol Buffers](https://developers.google.com/protocol-buffers)

### Enabling Protocol Buffers in a host

To use Protocol Buffers the infrastructure project should take a dependency on `GreenEnergyHub.Messaging.Protobuf`. In this assembly is there a default implementation of Protocol Buffers that follows the conceptual flow for sending and receiving messages. If a host can receive multiple messages through a single endpoint the message needs to be wrapped in an envelope. We have chosen to use the `Oneof` approach.

Example:

```proto
message Envelope {
    oneof Message {
        MoveIn moveIn = 1;
        MoveOut moveOut = 2;
    }
}

message MoveIn {
    // properties for MoveIn
}

message MoveOut {
    // properties for MoveOut
}
```

When this is compiled to C# it will produce three classes: `Envelope`, `MoveIn`, and `MoveOut`. The host needs to be configured to understand these classes so that it can deserialize them correctly.

#### Receiving data

If the host is to receive Protocol Buffers data it needs to be configured in the dependency injection system.

```csharp
services.ReceiveProtobuf<Envelope>(cfg => cfg.FromOneOf(msg => msg.MessageCases));
```

This will configure the host with a default parser. The parser is located with reflection by inspecting the Envelope class and extracting the property named `Parser`. This property is generated by the `protoc` tool and is expected to located on all classes generated by `protoc`. If for some reason it is needed to use another parser this can be achieved by setting one explicit.

```csharp
services.ReceiveProtobuf<Envelope>(cfg => cfg
    .WithParser(() => Envelope.Parser)
    .FromOneOf(msg => msg.MessageCases)
);
```

#### Sending data

If the host needs to send objects to other hosts as Protocol Buffers this also needs to be configured in the dependency injection.

```csharp
services.SendProtobuf<Envelope>();
```

### Separation of responsibility

All the work with Protocol Buffers is considered a technical detail. As a result of this, all the contracts should be placed in the infrastructure layer. In order to map from an application object to a Protocol Buffer object, a developer needs to provide a mapping class.

There are two kinds of maps.

- The `ProtobufInboundMapper` accepts a Protocol Buffer object and returns an application object
- The `ProtobufOutboundMapper` accepts an application object and returns an `IMessage` object that can be serialized

The implementations are discovered when the corresponding configuration is invoked on the dependency injection; `ReceiveProtobuf` method locates `ProtobufInboundMapper` implementations, `SendProtobuf` locates `ProtobufOutboundMapper` implementations.

### Tooling

In order to compile proto files to C# or Python a tool is needed. The compiler is known as `protoc` and can be installed either from the binaries provided by Google, or a package manager.

Using chocolatey (Windows): [chocolatey link](https://chocolatey.org/packages/protoc/3.14.0)

``` bash
choco install protoc
```

Using brew (Mac OS X): [brew link](https://github.com/protocolbuffers/protobuf/)

``` bash
brew install protobuf@3.14
```

#### Include tooling in `csproj`

It is possible to have the proto contracts generated at compile time by using a NuGet package - `Grpc.Tools v2.34.0`

When this is referenced in the `csproj` file

``` xml
<ItemGroup>
    <PackageReference Include="Grpc.Tools" Version="2.34.0" />
</ItemGroup>
```

Configuration of the utility also happens in the `csproj` file

``` xml
<ItemGroup>
    <Protobuf Include="proto-contracts\*.proto">
      <GrpcServices>None</GrpcServices>
      <Access>Public</Access>
      <ProtoCompile>True</ProtoCompile>
      <CompileOutputs>True</CompileOutputs>
      <Generator>MSBuild:Compile</Generator>
    </Protobuf>
</ItemGroup>
```

With the configuration in place, the contracts can be used from within C#.

The include attribute is a path to where the files can be located.
