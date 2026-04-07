using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Rinzler78.NetExtension.Network;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests for NetworkHelper that require real TCP sockets.
/// Kept separate from unit tests to avoid flakiness under parallel load.
/// </summary>
[Trait("Category", "Integration")]
public class NetworkHelperIntegrationTests
{
    [Fact]
    public async Task IsPortOpened_WithOpenAndClosedPorts_ShouldReflectSocketState()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var openPort = (uint)((IPEndPoint)listener.LocalEndpoint).Port;
        using var acceptLoopCts = new CancellationTokenSource();
        var acceptLoop = AcceptConnectionsUntilCancelledAsync(listener, acceptLoopCts.Token);

        // Small warmup to allow the OS to finish binding the listening socket before probing it.
        await Task.Delay(50);

        // Retry all open-port assertions to tolerate transient scheduler delays on CI runners.
        (await IsPortOpenedWithRetryAsync(IPAddress.Loopback, openPort)).Should().BeTrue(
            "uint overload: the listener is running and should be reachable on loopback");
        (await IsPortOpenedWithRetryIntAsync(IPAddress.Loopback, (int)openPort)).Should().BeTrue(
            "int overload: the listener should still be reachable");
        (await IsPortOpenedWithRetryAsync("127.0.0.1", openPort)).Should().BeTrue(
            "string overload: the listener should still be reachable");

        acceptLoopCts.Cancel();
        listener.Stop();
        await acceptLoop;

        (await IPAddress.Loopback.IsPortOpened(openPort)).Should().BeFalse();
    }

    /// <summary>
    /// Retries IsPortOpened up to <paramref name="maxAttempts"/> times with an
    /// increasing delay. This compensates for CI runners where loopback TCP
    /// connections may take slightly longer to settle than a single probe allows.
    /// </summary>
    private static async Task<bool> IsPortOpenedWithRetryAsync(IPAddress address, uint port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (await address.IsPortOpened(port))
            {
                return true;
            }

            await Task.Delay(100 * (attempt + 1));
        }

        return false;
    }

    private static async Task<bool> IsPortOpenedWithRetryIntAsync(IPAddress address, int port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (await address.IsPortOpened(port))
            {
                return true;
            }

            await Task.Delay(100 * (attempt + 1));
        }

        return false;
    }

    private static async Task<bool> IsPortOpenedWithRetryAsync(string host, uint port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (await host.IsPortOpened(port))
            {
                return true;
            }

            await Task.Delay(100 * (attempt + 1));
        }

        return false;
    }

    private static async Task AcceptConnectionsUntilCancelledAsync(TcpListener listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var acceptedClient = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
