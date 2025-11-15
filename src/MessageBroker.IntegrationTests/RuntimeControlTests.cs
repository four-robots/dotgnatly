using MessageBroker.Core.Configuration;
using MessageBroker.Nats.Implementation;

namespace MessageBroker.IntegrationTests;

/// <summary>
/// Integration test suite wrapper for runtime control tests.
/// </summary>
public class RuntimeControlTestSuite : IIntegrationTest
{
    public async Task RunAsync(TestResults results)
    {
        await results.AssertAsync("Get Server ID", RuntimeControlTests.TestGetServerId);
        await results.AssertAsync("Get Server Name", RuntimeControlTests.TestGetServerName);
        await results.AssertAsync("Is Server Running - True", RuntimeControlTests.TestIsServerRunning_True);
        await results.AssertAsync("Is Server Running - False", RuntimeControlTests.TestIsServerRunning_False);
    }
}

/// <summary>
/// Integration tests for NATS server runtime control (GetServerId, GetServerName, IsServerRunning).
/// </summary>
public static class RuntimeControlTests
{
    public static async Task<bool> TestGetServerId()
    {
        Console.WriteLine("\n=== Testing GetServerId (Server ID Retrieval) ===");

        using var controller = new NatsController();

        // Start server
        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4238,
            Description = "Server ID test"
        };

        var result = await controller.ConfigureAsync(config);
        if (!result.Success)
        {
            Console.WriteLine($"❌ Failed to start server: {result.ErrorMessage}");
            return false;
        }

        await Task.Delay(500);

        try
        {
            // Get server ID
            var serverId = await controller.GetServerIdAsync();
            Console.WriteLine($"✓ Retrieved server ID: {serverId}");

            if (string.IsNullOrWhiteSpace(serverId))
            {
                Console.WriteLine("❌ Server ID is null or empty");
                return false;
            }

            // Server ID should be a UUID-like string
            if (serverId.Length < 10)
            {
                Console.WriteLine($"❌ Server ID seems too short: {serverId}");
                return false;
            }

            Console.WriteLine("✓ GetServerId test passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetServerId test failed: {ex.Message}");
            return false;
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    public static async Task<bool> TestGetServerName()
    {
        Console.WriteLine("\n=== Testing GetServerName (Server Name Retrieval) ===");

        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4239,
            Description = "Server name test"
        };

        var result = await controller.ConfigureAsync(config);
        if (!result.Success)
        {
            Console.WriteLine($"❌ Failed to start server: {result.ErrorMessage}");
            return false;
        }

        await Task.Delay(500);

        try
        {
            // Get server name
            var serverName = await controller.GetServerNameAsync();
            Console.WriteLine($"✓ Retrieved server name: '{serverName}'");

            // Note: Server name can be empty if not configured
            // This is a valid scenario, so we just log it
            if (string.IsNullOrEmpty(serverName))
            {
                Console.WriteLine("  (Server name not configured - this is valid)");
            }

            Console.WriteLine("✓ GetServerName test passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetServerName test failed: {ex.Message}");
            return false;
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    public static async Task<bool> TestIsServerRunning_True()
    {
        Console.WriteLine("\n=== Testing IsServerRunning (Running State) ===");

        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4240,
            Description = "Server running test"
        };

        var result = await controller.ConfigureAsync(config);
        if (!result.Success)
        {
            Console.WriteLine($"❌ Failed to start server: {result.ErrorMessage}");
            return false;
        }

        await Task.Delay(500);

        try
        {
            // Check if server is running
            var isRunning = await controller.IsServerRunningAsync();
            Console.WriteLine($"✓ Server running status: {isRunning}");

            if (!isRunning)
            {
                Console.WriteLine("❌ Expected server to be running, but it reports as not running");
                return false;
            }

            Console.WriteLine("✓ IsServerRunning (true) test passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ IsServerRunning test failed: {ex.Message}");
            return false;
        }
        finally
        {
            await controller.ShutdownAsync();
        }
    }

    public static async Task<bool> TestIsServerRunning_False()
    {
        Console.WriteLine("\n=== Testing IsServerRunning (Not Running State) ===");

        using var controller = new NatsController();

        var config = new BrokerConfiguration
        {
            Host = "127.0.0.1",
            Port = 4241,
            Description = "Server not running test"
        };

        var result = await controller.ConfigureAsync(config);
        if (!result.Success)
        {
            Console.WriteLine($"❌ Failed to start server: {result.ErrorMessage}");
            return false;
        }

        await Task.Delay(500);

        try
        {
            // Shutdown the server
            await controller.ShutdownAsync();
            Console.WriteLine("✓ Server shut down");

            // Wait a bit for shutdown to complete
            await Task.Delay(200);

            // Check if server is running (should be false now)
            var isRunning = await controller.IsServerRunningAsync();
            Console.WriteLine($"✓ Server running status after shutdown: {isRunning}");

            if (isRunning)
            {
                Console.WriteLine("❌ Expected server to be not running after shutdown");
                return false;
            }

            Console.WriteLine("✓ IsServerRunning (false) test passed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ IsServerRunning (false) test failed: {ex.Message}");
            return false;
        }
    }
}
