# DotGnatly.JetStream

Fluent extension methods for managing NATS JetStream streams, consumers, and key-value stores using the official NATS.Net client library.

## Overview

This package extends `NatsController` with client-side JetStream operations, providing a seamless fluent API for stream management that works alongside DotGnatly's server management capabilities.

**Key Distinction:**
- **DotGnatly.Nats** - Server-side control (configures JetStream settings, storage limits, etc.)
- **DotGnatly.JetStream** - Client-side operations (creates/manages streams, consumers, etc.)

## Installation

```bash
dotnet add package DotGnatly.JetStream
```

This package depends on:
- `DotGnatly.Core` - Core abstractions
- `DotGnatly.Nats` - NATS server control
- `NATS.Net` - Official NATS client library

## Quick Start

```csharp
using DotGnatly.Core.Configuration;
using DotGnatly.JetStream.Extensions;
using DotGnatly.Nats.Implementation;
using NATS.Client.JetStream.Models;

// 1. Start server with JetStream enabled (server-side)
using var controller = new NatsController();
await controller.ConfigureAsync(new BrokerConfiguration
{
    Port = 4222,
    Jetstream = true,
    JetstreamStoreDir = "./jetstream"
});

// 2. Create a stream (client-side)
var streamInfo = await controller.CreateStreamAsync("ORDERS", builder =>
{
    builder
        .WithSubjects("orders.*")
        .WithDescription("Order events")
        .WithStorage(StreamConfigStorage.File)
        .WithMaxMessages(10000)
        .WithMaxAge(TimeSpan.FromDays(30));
});

// 3. List streams
var streams = await controller.ListStreamsAsync();
foreach (var name in streams)
{
    Console.WriteLine($"Stream: {name}");
}

// 4. Clean up
await controller.DeleteStreamAsync("ORDERS");
await controller.ShutdownAsync();
```

## Features

### Stream Management

**Create Stream:**
```csharp
await controller.CreateStreamAsync("EVENTS", builder =>
{
    builder
        .WithSubjects("events.>")
        .WithStorage(StreamConfigStorage.Memory)
        .WithRetention(StreamConfigRetention.Limits)
        .WithMaxAge(TimeSpan.FromHours(1));
});
```

**Update Stream:**
```csharp
await controller.UpdateStreamAsync("EVENTS", builder =>
{
    builder
        .WithSubjects("events.>")
        .WithMaxMessages(20000); // Updated limit
});
```

**Get Stream Info:**
```csharp
var info = await controller.GetStreamAsync("EVENTS");
Console.WriteLine($"Messages: {info.State.Messages}");
Console.WriteLine($"Bytes: {info.State.Bytes}");
```

**List Streams:**
```csharp
var streams = await controller.ListStreamsAsync();
```

**Delete Stream:**
```csharp
await controller.DeleteStreamAsync("EVENTS");
```

**Purge Stream:**
```csharp
var response = await controller.PurgeStreamAsync("EVENTS");
Console.WriteLine($"Purged {response.Purged} messages");
```

### Stream Configuration Options

The `StreamConfigBuilder` provides a fluent API for configuring streams:

```csharp
builder
    .WithSubjects("orders.*", "shipments.*")     // Subject patterns
    .WithDescription("Order processing")          // Description
    .WithStorage(StreamConfigStorage.File)        // File or Memory
    .WithRetention(StreamConfigRetention.Limits)  // Limits, Interest, or WorkQueue
    .WithReplicas(3)                              // Cluster replicas (1-5)
    .WithMaxMessages(100000)                      // Max message count
    .WithMaxBytes(1024 * 1024 * 1024)            // Max storage (1GB)
    .WithMaxAge(TimeSpan.FromDays(7))            // Message TTL
    .WithMaxMessageSize(1024 * 1024)             // Max individual message size
    .WithMaxConsumers(10)                         // Max consumer count
    .WithDiscard(StreamConfigDiscard.Old)         // Discard old or new when full
    .WithDuplicateWindow(TimeSpan.FromMinutes(5)) // Duplicate detection window
    .WithNoAck();                                 // Disable acknowledgements
```

### Storage Types

- **File** - Persistent storage on disk (default)
- **Memory** - In-memory storage (fast, non-persistent)

### Retention Policies

- **Limits** - Keep messages until limits are reached (max messages, bytes, or age)
- **Interest** - Keep messages while there are active consumers
- **WorkQueue** - Delete messages after acknowledgement (work queue pattern)

### Discard Policies

- **Old** - Discard oldest messages when limits are reached (default)
- **New** - Reject new messages when limits are reached

## Advanced Usage

### Direct JetStream Context

For advanced scenarios, you can get a JetStream context directly:

```csharp
await using var context = await controller.GetJetStreamContextAsync();

// Use the NATS.Net JetStream API directly
var stream = await context.JetStream.GetStreamAsync("ORDERS");
var consumer = await stream.CreateConsumerAsync();

// Context is automatically disposed when done
```

### Authentication

The extension methods automatically use authentication configured in `BrokerConfiguration`:

```csharp
await controller.ConfigureAsync(new BrokerConfiguration
{
    Port = 4222,
    Jetstream = true,
    Auth = new AuthConfiguration
    {
        Username = "admin",
        Password = "secret"
    }
});

// Stream operations will use the configured credentials
await controller.CreateStreamAsync("SECURE", builder => { ... });
```

## Examples

See the [StreamManagementExample.cs](../../DotGnatly.Examples/JetStream/StreamManagementExample.cs) for a complete working example demonstrating:
- Creating streams with different configurations
- Updating stream settings
- Listing and querying streams
- Purging and deleting streams
- Cleanup and resource management

## Architecture

```
┌─────────────────────────────────────┐
│   DotGnatly.JetStream Extensions    │
│  (Stream Management - Client Side)  │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│        NATS.Net Client Library      │
│     (Official NATS Client v2)       │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│      DotGnatly.Nats Controller      │
│   (Server Management - Server Side) │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│         NATS Server Process         │
│      (with JetStream enabled)       │
└─────────────────────────────────────┘
```

## Why a Separate Package?

The JetStream extension is a separate package because:

1. **Separation of Concerns** - Server control (DotGnatly.Nats) vs. client operations (DotGnatly.JetStream)
2. **Optional Dependency** - Not all users need stream management capabilities
3. **Clean Dependencies** - Avoids forcing NATS.Net client dependency on server-only users
4. **Independent Versioning** - Client and server packages can evolve independently

## NuGet Package

```xml
<PackageReference Include="DotGnatly.JetStream" Version="1.0.0" />
```

This will automatically bring in:
- DotGnatly.Core
- DotGnatly.Nats
- DotGnatly.Natives (platform-specific)
- NATS.Net

## License

MIT License - Copyright (c) 2024 DotGnatly Contributors

## Links

- [DotGnatly Documentation](../../docs/)
- [NATS.Net Documentation](https://github.com/nats-io/nats.net)
- [JetStream Concepts](https://docs.nats.io/nats-concepts/jetstream)
- [GitHub Repository](https://github.com/four-robots/messagebroker.net)
