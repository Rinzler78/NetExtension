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

        // Use retry for all open-port assertions to tolerate slow CI runners (200 ms TCP timeout).
        // Small warmup to allow the OS to finish binding the listening socket before probing it.
        await Task.Delay(50);

        // Use retry for all open-port assertions to tolerate slow CI runners.
        // Retrying avoids false negatives caused by transient OS scheduler delays without hiding real bugs.
        IsPortOpenedWithRetry(IPAddress.Loopback, openPort).Should().BeTrue(
            "uint overload: the listener is running and should be reachable on loopback");
        IsPortOpenedWithRetryInt(IPAddress.Loopback, (int)openPort).Should().BeTrue(
            "int overload: the listener should still be reachable");
        IsPortOpenedWithRetry("127.0.0.1", openPort).Should().BeTrue(
            "string overload: the listener should still be reachable");

        acceptLoopCts.Cancel();
        listener.Stop();
        await acceptLoop;

        IPAddress.Loopback.IsPortOpened(openPort).Should().BeFalse();
    }

    /// <summary>
    /// Retries IsPortOpened up to <paramref name="maxAttempts"/> times with an
    /// increasing delay. This compensates for CI runners where loopback TCP
    /// connections may take slightly longer than the library's 200 ms timeout.
    /// </summary>
    private static bool IsPortOpenedWithRetry(IPAddress address, uint port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (address.IsPortOpened(port))
            {
                return true;
            }

            Thread.Sleep(100 * (attempt + 1));
        }

        return false;
    }

    private static bool IsPortOpenedWithRetryInt(IPAddress address, int port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (address.IsPortOpened(port))
            {
                return true;
            }

            Thread.Sleep(100 * (attempt + 1));
        }

        return false;
    }

    private static bool IsPortOpenedWithRetry(string host, uint port, int maxAttempts = 5)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (host.IsPortOpened(port))
            {
                return true;
            }

            Thread.Sleep(100 * (attempt + 1));
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
