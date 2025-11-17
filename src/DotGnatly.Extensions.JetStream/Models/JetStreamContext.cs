using NATS.Client.Core;
using NATS.Client.JetStream;

namespace DotGnatly.Extensions.JetStream.Models;

/// <summary>
/// Encapsulates a NATS connection and JetStream context for managing streams and consumers.
/// Implements IAsyncDisposable to properly clean up the connection when done.
/// </summary>
public class JetStreamContext : IAsyncDisposable
{
    private readonly NatsConnection _connection;

    /// <summary>
    /// Gets the JetStream context for stream and consumer operations.
    /// </summary>
    public INatsJSContext JetStream { get; }

    /// <summary>
    /// Gets the underlying NATS connection.
    /// </summary>
    public NatsConnection Connection => _connection;

    /// <summary>
    /// Initializes a new instance of the JetStreamContext class.
    /// </summary>
    /// <param name="connection">The NATS connection.</param>
    /// <param name="jetStream">The JetStream context.</param>
    internal JetStreamContext(NatsConnection connection, INatsJSContext jetStream)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        JetStream = jetStream ?? throw new ArgumentNullException(nameof(jetStream));
    }

    /// <summary>
    /// Disposes the JetStream context and closes the NATS connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
