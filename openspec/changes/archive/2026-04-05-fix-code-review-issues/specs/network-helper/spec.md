## MODIFIED Requirements

### Requirement: Port Open Check
`IsPortOpened(this IPAddress, uint portNumber)` SHALL attempt a TCP connection to the
given address and port with a 1-second timeout, returning `true` if the connection
succeeds and `false` if it times out or is refused.
Port values greater than 65535 SHALL throw `ArgumentOutOfRangeException`.
Port values of 0 are allowed (system assigned).

The return type is changed from `bool` to `Task<bool>`. **BREAKING** — pre-1.0.

#### Scenario: Open port
- **WHEN** a TCP server is listening on the target address and port
- **THEN** `IsPortOpened` resolves to `true`

#### Scenario: Closed port
- **WHEN** no service is listening on the port
- **THEN** `IsPortOpened` resolves to `false` within 1 second

#### Scenario: Invalid port number
- **WHEN** `portNumber` exceeds 65535
- **THEN** `ArgumentOutOfRangeException` is thrown

### Requirement: Modern Async TCP Connection
The TCP connection check SHALL use `ConnectAsync` with `CancellationTokenSource.CancelAfter`
rather than the deprecated APM `BeginConnect`/`EndConnect`/`AsyncWaitHandle.WaitOne` pattern.

#### Scenario: Uses ConnectAsync
- **WHEN** `IsPortOpened` is called
- **THEN** the connection attempt uses `Socket.ConnectAsync(EndPoint, CancellationToken)`

#### Scenario: Timeout via CancellationToken
- **WHEN** the connection attempt exceeds `PortCheckTimeoutMs` (1000ms)
- **THEN** the `CancellationToken` fires and the socket is disposed, returning `false`
