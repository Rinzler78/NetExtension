using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Rinzler78.NetExtension.Network;

/// <summary>
/// Provides extension methods for network operations: ping, host resolution, and port checking.
/// </summary>
public static class NetworkHelper
{
    #region Constants

    /// <summary>Connection timeout for port checking operations in milliseconds.</summary>
    private const int PortCheckTimeoutMs = 1000;

    #endregion

    /// <summary>Sends an ICMP echo request to <paramref name="hostName"/> and returns the reply.</summary>
    /// <param name="hostName">The host to ping.</param>
    /// <returns>The <see cref="PingReply"/> from the target host.</returns>
    public static PingReply Ping(this string hostName)
    {
        using var ping = new Ping();
        return ping.Send(hostName);
    }

    /// <summary>
    /// Resolves a hostname to its first <see cref="IPAddress"/>.
    /// </summary>
    /// <param name="host">The hostname to resolve. Must not be null.</param>
    /// <returns>
    /// The first resolved <see cref="IPAddress"/>, or <see langword="null"/> when
    /// <paramref name="host"/> is empty or DNS returns no addresses.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null.</exception>
    /// <exception cref="SocketException">Thrown when DNS resolution fails.</exception>
    public static IPAddress? Resolve(this string host)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (host.Length > 0)
        {
            var addresses = Dns.GetHostAddresses(host);
            return addresses.Length > 0 ? addresses[0] : null;
        }

        return null;
    }

    /// <summary>
    /// Checks whether the specified port is open on <paramref name="host"/>.
    /// </summary>
    /// <param name="host">The hostname or IP string to check.</param>
    /// <param name="portNumber">The port number (0–65535).</param>
    /// <returns>
    /// <see langword="true"/> if the port is open; <see langword="false"/> otherwise
    /// or when the host cannot be resolved.
    /// </returns>
    public static async Task<bool> IsPortOpened(this string host, uint portNumber)
    {
        if (host?.Length > 0 && portNumber <= ushort.MaxValue)
        {
            var ip = host.Resolve();
            if (ip is null) return false;
            return await ip.IsPortOpened(portNumber).ConfigureAwait(false);
        }

        return false;
    }

    /// <summary>
    /// Checks whether the specified port is open on <paramref name="ipAddress"/>.
    /// </summary>
    /// <param name="ipAddress">The IP address to check. Must not be null.</param>
    /// <param name="portNumber">The port number as a signed integer (0–65535).</param>
    /// <returns><see langword="true"/> if the port is open; <see langword="false"/> otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="portNumber"/> is negative or exceeds 65535.
    /// </exception>
    public static Task<bool> IsPortOpened(this IPAddress ipAddress, int portNumber)
    {
        if (portNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");
        return ipAddress.IsPortOpened((uint)portNumber);
    }

    /// <summary>
    /// Checks whether the specified port is open on <paramref name="ipAddress"/>
    /// using an async TCP connect with a <see cref="PortCheckTimeoutMs"/> ms timeout.
    /// </summary>
    /// <param name="ipAddress">The IP address to check. Must not be null.</param>
    /// <param name="portNumber">The port number (0–65535).</param>
    /// <returns><see langword="true"/> if the port is open; <see langword="false"/> if refused or timed out.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ipAddress"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="portNumber"/> exceeds 65535.</exception>
    public static async Task<bool> IsPortOpened(this IPAddress ipAddress, uint portNumber)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (portNumber > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");

        using var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        using var cts = new CancellationTokenSource(PortCheckTimeoutMs);

        try
        {
            await socket.ConnectAsync(new IPEndPoint(ipAddress, (int)portNumber), cts.Token)
                .ConfigureAwait(false);
            return socket.Connected;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
