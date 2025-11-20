using DotGnatly.Core.Configuration;
using DotGnatly.Nats.Implementation;
using Xunit;

namespace DotGnatly.IntegrationTests;

/// <summary>
/// Integration tests for NATS server runtime control (GetServerId, GetServerName, IsServerRunning).
/// </summary>
public class RuntimeControlTests
{
    [Fact]
    public async Task TestGetServerId()
    {
        using var controller = new NatsController();

        // Start server
        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4238,
            Description = "Server ID test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        try
        {
            // Get server ID
            var serverId = await controller.GetServerIdAsync();

            Assert.False(string.IsNullOrWhiteSpace(serverId), "Server ID should not be null or empty");

            // Server ID should be a UUID-like string
            Assert.True(serverId.Length >= 10, $"Server ID seems too short: {serverId}");
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    [Fact]
    public async Task TestGetServerName()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4239,
            Description = "Server name test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        try
        {
            // Get server name
            var serverName = await controller.GetServerNameAsync();

            // Note: Server name can be empty if not configured
            // This is a valid scenario, so we just verify the call doesn't throw
            Assert.NotNull(serverName);
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    [Fact]
    public async Task TestIsServerRunning_True()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4240,
            Description = "Server running test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        try
        {
            // Check if server is running
            var isRunning = await controller.IsServerRunningAsync();

            Assert.True(isRunning, "Expected server to be running, but it reports as not running");
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    [Fact]
    public async Task TestIsServerRunning_False()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4241,
            Description = "Server not running test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        // Shutdown the server
        await controller.ShutdownAsync();

        // Wait a bit for shutdown to complete
        await Task.Delay(200);

        // Check if server is running (should be false now)
        var isRunning = await controller.IsServerRunningAsync();

        Assert.False(isRunning, "Expected server to be not running after shutdown");
    }

    [Fact]
    public async Task TestWaitForReady()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4242,
            Description = "Wait for ready test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        try
        {
            // Wait for server to be ready with a 5 second timeout
            var isReady = await controller.WaitForReadyAsync(timeoutSeconds: 5);

            Assert.True(isReady, "Expected server to be ready within timeout");
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    [Fact]
    public async Task TestIsJetStreamEnabled_False()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4243,
            Jetstream = false, // Explicitly disable JetStream
            Description = "JetStream disabled test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        try
        {
            // Check if JetStream is enabled
            var isEnabled = await controller.IsJetStreamEnabledAsync();

            Assert.False(isEnabled, "Expected JetStream to be disabled");
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    [Fact]
    public async Task TestIsJetStreamEnabled_True()
    {
        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4244,
            Jetstream = true, // Enable JetStream
            JetstreamStoreDir = Path.Combine(Path.GetTempPath(), "nats-js-test"),
            Description = "JetStream enabled test"
        };

        var result = await controller.ConfigureAsync(config);
        Assert.True(result.Success, $"Failed to start server: {result.ErrorMessage}");

        await Task.Delay(500);

        try
        {
            // Check if JetStream is enabled
            var isEnabled = await controller.IsJetStreamEnabledAsync();

            Assert.True(isEnabled, "Expected JetStream to be enabled");
        }
        finally
        {
            await controller.ShutdownAsync();

            // Clean up JetStream store directory
            try
            {
                if (Directory.Exists(config.JetstreamStoreDir))
                {
                    Directory.Delete(config.JetstreamStoreDir, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
