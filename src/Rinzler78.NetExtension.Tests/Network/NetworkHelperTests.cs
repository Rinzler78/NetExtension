using System.Net;
using System.Net.Sockets;
using Rinzler78.NetExtension.Network;

namespace Rinzler78.NetExtension.Tests.Network;

[Trait("Category", "Unit")]
public class NetworkHelperTests
{
    [Fact]
    public void Resolve_WithEmptyHost_ShouldReturnNull()
    {
        "".Resolve().Should().BeNull();
    }

    [Fact]
    public void Resolve_WithNullHost_ShouldThrowArgumentNullException()
    {
        Action act = () => NetworkHelper.Resolve(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Resolve_WithLocalhost_ShouldReturnAddress()
    {
        var result = "localhost".Resolve();

        result.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_WithUnknownHost_ShouldThrowSocketException()
    {
        Action act = () => "nonexistent-host.invalid".Resolve();

        act.Should().Throw<SocketException>();
    }

    [Fact]
    public void Ping_WithLoopback_ShouldReturnReply()
    {
        var reply = "127.0.0.1".Ping();

        reply.Should().NotBeNull();
    }

    [Fact]
    public void IsPortOpened_WithInvalidArguments_ShouldReturnFalseOrThrow()
    {
        "".IsPortOpened(1234).Should().BeFalse();
        "localhost".IsPortOpened((uint)ushort.MaxValue + 1).Should().BeFalse();

        Action act = () => IPAddress.Loopback.IsPortOpened((uint)ushort.MaxValue + 1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IsPortOpened_WithNullIp_ShouldThrowArgumentNullException()
    {
        Action act = () => NetworkHelper.IsPortOpened((IPAddress)null!, 80u);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IsPortOpened_WithOpenAndClosedPorts_ShouldReflectSocketState()
    {
        // Moved to Integration/NetworkHelperIntegrationTests.cs — uses real TCP socket
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsPortOpened_WithMaxPort_ShouldNotReturnFalseForInvalidReason()
    {
        // Port 65535 must be accepted as valid (must not return false due to validation)
        // Cannot open a real socket here, but we verify the call does not throw
        var act = () => IPAddress.Loopback.IsPortOpened(65535u);
        act.Should().NotThrow<ArgumentOutOfRangeException>("port 65535 is a valid TCP port");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsPortOpened_WithNegativePortNumber_ShouldThrowArgumentOutOfRangeException()
    {
        var act = () => IPAddress.Loopback.IsPortOpened(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
