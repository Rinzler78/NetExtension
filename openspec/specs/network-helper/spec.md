# Capability: Network Helper

## Purpose
Provides extension methods for network operations: host resolution, port checking,
and ICMP ping. All operations use modern async-compatible APIs.

## Requirements

### Requirement: Port Open Check
`IsPortOpened(this IPAddress, uint portNumber)` SHALL attempt a TCP connection to the
given address and port with a 1-second timeout, returning a `Task<bool>` that resolves
to `true` if the connection succeeds and `false` if it times out or is refused.
Port values greater than 65535 SHALL throw `ArgumentOutOfRangeException`.

#### Scenario: Open port
- **WHEN** a TCP server is listening on the target address and port
- **THEN** the returned task resolves to `true`

#### Scenario: Closed port
- **WHEN** no service is listening on the port
- **THEN** the returned task resolves to `false` within the 1-second timeout

#### Scenario: Invalid port number
- **WHEN** `portNumber` exceeds 65535
- **THEN** `ArgumentOutOfRangeException` is thrown

### Requirement: Modern Async TCP Connection
The TCP connection check SHALL use `ConnectAsync` with a `CancellationToken` rather
than the deprecated APM `BeginConnect`/`EndConnect` pattern.

#### Scenario: Uses ConnectAsync
- **WHEN** `IsPortOpened` is called
- **THEN** the connection attempt uses `Socket.ConnectAsync(EndPoint, CancellationToken)`

### Requirement: Host Resolution
`Resolve(this string host)` SHALL return the first `IPAddress` from DNS for the
given hostname, or `null` if the hostname is empty or DNS returns no addresses.
Throws `ArgumentNullException` for null input; throws `SocketException` on DNS failure.

#### Scenario: Known hostname
- **WHEN** `Resolve` is called with a valid hostname
- **THEN** a non-null `IPAddress` is returned

#### Scenario: Empty host
- **WHEN** `Resolve` is called with an empty string
- **THEN** `null` is returned

### Requirement: Ping
`Ping(this string hostName)` SHALL send an ICMP echo request and return the `PingReply`.
The `Ping` instance SHALL be disposed after use.

#### Scenario: Successful ping
- **WHEN** called with a reachable hostname
- **THEN** a `PingReply` with status `Success` is returned
