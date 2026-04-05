using System;
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
    public async Task IsPortOpened_WithInvalidArguments_ShouldReturnFalseOrThrow()
    {
        (await "".IsPortOpened(1234)).Should().BeFalse();
        (await "localhost".IsPortOpened((uint)ushort.MaxValue + 1)).Should().BeFalse();

        Func<Task> act = () => IPAddress.Loopback.IsPortOpened((uint)ushort.MaxValue + 1);
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task IsPortOpened_WithNullIp_ShouldThrowArgumentNullException()
    {
        Func<Task> act = () => NetworkHelper.IsPortOpened((IPAddress)null!, 80u);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task IsPortOpened_WithMaxPort_ShouldNotReturnFalseForInvalidReason()
    {
        // Port 65535 is valid. This unit test only verifies validation, not socket reachability.
        Func<Task> act = async () => await IPAddress.Loopback.IsPortOpened(65535u);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void IsPortOpened_WithNegativePortNumber_ShouldThrowArgumentOutOfRangeException()
    {
        // int overload throws synchronously before returning a Task
        Action act = () => IPAddress.Loopback.IsPortOpened(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task IsPortOpened_NonRoutableAddress_ReturnsFalseWithinTimeout()
    {
        // 192.0.2.0/24 is TEST-NET-1 — non-routable, will always time out
        var ip = IPAddress.Parse("192.0.2.1");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await ip.IsPortOpened(9999u);

        sw.Stop();
        result.Should().BeFalse();
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }
}
