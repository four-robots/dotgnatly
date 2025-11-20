using DotGnatly.Core.Configuration;
using DotGnatly.Nats.Implementation;
using Xunit;

namespace DotGnatly.IntegrationTests;

/// <summary>
/// Tests hot reload functionality for various configuration scenarios.
/// </summary>
public class ConfigurationReloadTests
{
    [Fact]
    public async Task BasicHotReloadChangesConfiguration()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222, Debug = false });

        var result = await server.ApplyChangesAsync(c => c.Debug = true);

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(result.Success);
        Assert.True(info.CurrentConfig.Debug);
    }

    [Fact]
    public async Task HotReloadMultiplePropertiesSimultaneously()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration
        {
            Port = 14222,
            Debug = false,
            Trace = false,
            MaxPayload = 1024
        });

        var result = await server.ApplyChangesAsync(c =>
        {
            c.Debug = true;
            c.Trace = true;
            c.MaxPayload = 2048;
        });

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(result.Success);
        Assert.True(info.CurrentConfig.Debug);
        Assert.True(info.CurrentConfig.Trace);
        Assert.Equal(2048, info.CurrentConfig.MaxPayload);
    }

    [Fact]
    public async Task HotReloadIncrementsVersionNumber()
    {
        using var server = new NatsController();
        var result1 = await server.ConfigureAsync(new BrokerConfiguration { Port = 14222 });
        var version1 = result1.Version?.Version ?? 0;

        var result2 = await server.ApplyChangesAsync(c => c.Debug = true);
        var version2 = result2.Version?.Version ?? 0;

        await server.ShutdownAsync();

        Assert.True(version2 > version1);
    }

    [Fact]
    public async Task RollbackRestoresPreviousConfiguration()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222, Debug = false });

        await server.ApplyChangesAsync(c => c.Debug = true);

        var rollbackResult = await server.RollbackAsync(toVersion: 1);

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(rollbackResult.Success);
        Assert.False(info.CurrentConfig.Debug);
    }

    [Fact]
    public async Task MultipleSequentialHotReloadsWorkCorrectly()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222, MaxPayload = 1024 });

        for (int i = 1; i <= 10; i++)
        {
            await server.ApplyChangesAsync(c => c.MaxPayload = 1024 * (i + 1));
        }

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.Equal(1024 * 11, info.CurrentConfig.MaxPayload);
    }

    [Fact]
    public async Task HotReloadWithValidationFailurePreservesOriginalConfig()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222, MaxPayload = 1024 });

        var result = await server.ApplyChangesAsync(c => c.MaxPayload = -1);

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.False(result.Success);
        Assert.Equal(1024, info.CurrentConfig.MaxPayload);
    }

    [Fact]
    public async Task JetStreamCanBeToggledViaHotReload()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration
        {
            Port = 14222,
            Jetstream = false
        });

        var result = await server.ApplyChangesAsync(c =>
        {
            c.Jetstream = true;
            c.JetstreamStoreDir = "./jetstream-test";
        });

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(info.CurrentConfig.Jetstream);
    }

    [Fact]
    public async Task VersionNumberIncrementsWithEachChange()
    {
        using var server = new NatsController();
        var result = await server.ConfigureAsync(new BrokerConfiguration { Port = 14222 });
        var initialVersion = result.Version?.Version ?? 0;

        ConfigurationResult? lastResult = null;
        for (int i = 0; i < 5; i++)
        {
            lastResult = await server.ApplyChangesAsync(c => c.MaxPayload = 1024 + (i * 100));
        }

        await server.ShutdownAsync();

        Assert.NotNull(lastResult);
        Assert.NotNull(lastResult.Version);
        Assert.True(lastResult.Version.Version >= initialVersion + 5);
    }

    [Fact]
    public async Task LeafNodeConfigurationChangesRequireRestart()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration
        {
            Port = 14222,
            LeafNode = new LeafNodeConfiguration
            {
                Port = 0,
                ImportSubjects = new List<string>()
            }
        });

        // Attempt to hot reload LeafNode configuration
        var result = await server.ApplyChangesAsync(c =>
        {
            c.LeafNode.Port = 17422;
            c.LeafNode.ImportSubjects.Add("test.>");
        });

        await server.ShutdownAsync();

        // NATS server does not support hot reloading LeafNode configuration
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("LeafNode", result.ErrorMessage);
    }

    [Fact]
    public async Task HotReloadPreservesUnmodifiedProperties()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration
        {
            Port = 14222,
            Debug = false,
            Trace = false,
            MaxPayload = 1024,
            MaxControlLine = 4096
        });

        await server.ApplyChangesAsync(c => c.Debug = true);

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(info.CurrentConfig.Debug);
        Assert.False(info.CurrentConfig.Trace);
        Assert.Equal(1024, info.CurrentConfig.MaxPayload);
        Assert.Equal(4096, info.CurrentConfig.MaxControlLine);
    }

    [Fact]
    public async Task FluentApiExtensionsWorkForHotReload()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222 });

        await server.SetDebugAsync(true);
        await server.SetMaxPayloadAsync(2048);

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.True(info.CurrentConfig.Debug);
        Assert.Equal(2048, info.CurrentConfig.MaxPayload);
    }

    [Fact]
    public async Task AuthenticationCanBeChangedViaHotReload()
    {
        using var server = new NatsController();
        await server.ConfigureAsync(new BrokerConfiguration { Port = 14222 });

        await server.SetAuthenticationAsync("user", "pass");

        var info = await server.GetInfoAsync();
        await server.ShutdownAsync();

        Assert.Equal("user", info.CurrentConfig.Auth.Username);
        Assert.Equal("pass", info.CurrentConfig.Auth.Password);
    }
}
