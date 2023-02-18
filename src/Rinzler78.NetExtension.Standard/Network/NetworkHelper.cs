using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Rinzler78.NetExtension.Network;

public static class NetworkHelper
{
    public static PingReply Ping(this string hostName)
    {
        return new Ping().Send(hostName);
    }

    public static IPAddress Resolve(this string host)
    {
        if (host?.Length > 0)
            return Dns.GetHostAddresses(host)[0];

        return null;
    }

    public static bool IsPortOpened(this string host, uint portNumber)
    {
        if (host?.Length > 0 && portNumber < ushort.MaxValue)
            return host.Resolve().IsPortOpened(portNumber);

        return false;
    }

    public static bool IsPortOpened(this IPAddress iPAddress, int portNumber)
        => iPAddress.IsPortOpened((uint)portNumber);

    public static bool IsPortOpened(this IPAddress iPAddress, uint portNumber)
    {
        if (iPAddress != null)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);

            try
            {
                IAsyncResult result = socket.BeginConnect(iPAddress, (int)portNumber, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(200, true);

                if (socket.Connected)
                {
                    socket.EndConnect(result);
                    return true;
                }

                socket.Close();
                throw new ApplicationException("Failed to connect server.");
            }
            catch (Exception ex)
            {

            }
        }

        return false;
    }
}

