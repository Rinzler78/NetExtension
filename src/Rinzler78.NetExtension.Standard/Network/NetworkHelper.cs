using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Rinzler78.NetExtension.Network;

public static class NetworkHelper
{
    #region Constants

    /// <summary>
    /// Connection timeout for port checking operations in milliseconds.
    /// </summary>
    private const int PortCheckTimeoutMs = 1000;

    #endregion
    public static PingReply Ping(this string hostName)
    {
        using var ping = new Ping();
        return ping.Send(hostName);
    }

    /// <summary>
    /// Resolves a hostname to an IP address.
    /// </summary>
    /// <param name="host">The hostname to resolve.</param>
    /// <returns>
    /// The first resolved IP address, or <see langword="null"/> when <paramref name="host"/> is empty
    /// or DNS resolution returns no addresses.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when host is null.</exception>
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

    public static bool IsPortOpened(this string host, uint portNumber)
    {
        if (host?.Length > 0 && portNumber <= ushort.MaxValue)
            return host.Resolve()?.IsPortOpened(portNumber) ?? false;

        return false;
    }

    public static bool IsPortOpened(this IPAddress ipAddress, int portNumber)
    {
        if (portNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");
        return ipAddress.IsPortOpened((uint)portNumber);
    }

    /// <summary>
    /// Checks if a specific port is open on the given IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <param name="portNumber">The port number to check.</param>
    /// <returns>True if the port is open, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when ipAddress is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when portNumber is outside valid range.</exception>
    public static bool IsPortOpened(this IPAddress ipAddress, uint portNumber)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (portNumber > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");

        Socket? socket = null;
        try
        {
            socket = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);

            IAsyncResult result = socket.BeginConnect(ipAddress, (int)portNumber, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(PortCheckTimeoutMs, true);

            if (!success)
                return false;

            socket.EndConnect(result);
            return socket.Connected;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        finally
        {
            socket?.Close();
        }
    }
}
