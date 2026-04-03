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
    /// <returns>The resolved IP address, or null if resolution fails.</returns>
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

    public static bool IsPortOpened(this IPAddress iPAddress, int portNumber)
    {
        if (portNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");
        return iPAddress.IsPortOpened((uint)portNumber);
    }

    /// <summary>
    /// Checks if a specific port is open on the given IP address.
    /// </summary>
    /// <param name="iPAddress">The IP address to check.</param>
    /// <param name="portNumber">The port number to check.</param>
    /// <returns>True if the port is open, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when iPAddress is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when portNumber is outside valid range.</exception>
    public static bool IsPortOpened(this IPAddress iPAddress, uint portNumber)
    {
        ArgumentNullException.ThrowIfNull(iPAddress);

        if (portNumber > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(portNumber), "Port number must be between 0 and 65535");

        Socket? socket = null;
        try
        {
            socket = new Socket(iPAddress.AddressFamily,
                        SocketType.Stream,
                        ProtocolType.Tcp);

            IAsyncResult result = socket.BeginConnect(iPAddress, (int)portNumber, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(PortCheckTimeoutMs, true);

            if (!success)
                return false;

            socket.EndConnect(result);
            return socket.Connected;
        }
        catch (SocketException)
        {
            // Connection failed - port is closed
            return false;
        }
        catch (ObjectDisposedException)
        {
            // Socket was disposed
            return false;
        }
        finally
        {
            socket?.Close();
        }
    }
}
